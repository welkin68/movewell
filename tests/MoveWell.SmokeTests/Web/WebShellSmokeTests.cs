using System.Net;
using System.Text.RegularExpressions;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.SmokeTests.Infrastructure;

namespace MoveWell.SmokeTests.Web;

/// <summary>Web shell + BFF account flows through real HTML forms (Web → Api → LocalDB).</summary>
public sealed partial class WebShellSmokeTests : IClassFixture<SiteFixture>
{
    private const string Password = "Movewell123";
    private readonly SiteFixture _site;

    public WebShellSmokeTests(SiteFixture site)
    {
        _site = site;
    }

    [Fact]
    public async Task Persian_home_is_rtl_with_english_alternate()
    {
        var html = await _site.Browser().GetPageAsync("/");

        Assert.Contains("<html lang=\"fa\" dir=\"rtl\">", html);
        Assert.Contains("hreflang=\"en\" href=\"http://localhost/en\"", html);
        Assert.Contains("حرکت درست را یاد بگیرید", html);
    }

    [Fact]
    public async Task English_home_is_ltr_and_links_keep_the_en_prefix()
    {
        var html = await _site.Browser().GetPageAsync("/en");

        Assert.Contains("<html lang=\"en\" dir=\"ltr\">", html);
        Assert.Contains("Learn to move well", html);
        Assert.Contains("href=\"/en/account/signin\"", html);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/en")]
    [InlineData("/account/register")]
    [InlineData("/account/signin")]
    [InlineData("/admin/account/signin")]
    public async Task Pages_load_no_external_assets_and_send_a_csp(string url)
    {
        var response = await _site.Browser().GetAsync(url);
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(ExternalAsset().Matches(html));
        Assert.Contains("default-src 'self'", response.Headers.GetValues("Content-Security-Policy").Single());
    }

    [Fact]
    public async Task Unknown_page_shows_localized_404()
    {
        var html = await _site.Browser().GetPageAsync("/en/nope", HttpStatusCode.NotFound);

        Assert.Contains("Page not found", html);
    }

    [Fact]
    public async Task Register_through_web_forms_signs_the_member_in()
    {
        var browser = _site.Browser();
        var mobile = ApiClientExtensions.NewMobile();

        var start = await browser.SubmitAsync("/account/register", "/account/register", new Dictionary<string, string> { ["Mobile"] = mobile });
        Assert.Equal(HttpStatusCode.Redirect, start.StatusCode);
        Assert.Equal("/account/verify", start.RedirectPath());

        var verifyPage = await browser.GetPageAsync("/account/verify");
        Assert.DoesNotContain(mobile, verifyPage); // only the masked number is shown

        var code = _site.Api.Sms.LastCode(mobile, OtpPurpose.Registration)!;
        var complete = await browser.SubmitAsync("/account/verify", "/account/verify", new Dictionary<string, string>
        {
            ["Code"] = code,
            ["DisplayName"] = "نگار تست",
            ["Password"] = Password,
            ["ConfirmPassword"] = Password,
        });
        Assert.Equal(HttpStatusCode.Redirect, complete.StatusCode);
        Assert.Equal("/", complete.RedirectPath());
        Assert.Contains(complete.Headers.GetValues("Set-Cookie"), cookie => cookie.StartsWith(".movewell.member=", StringComparison.Ordinal));

        var home = await browser.GetPageAsync("/");
        Assert.Contains("نگار تست", home);
        Assert.Contains("خوش آمدید", home);
    }

    [Fact]
    public async Task Mismatched_passwords_are_caught_before_calling_the_api()
    {
        var browser = _site.Browser();
        var mobile = ApiClientExtensions.NewMobile();
        await browser.SubmitAsync("/account/register", "/account/register", new Dictionary<string, string> { ["Mobile"] = mobile });

        var response = await browser.SubmitAsync("/account/verify", "/account/verify", new Dictionary<string, string>
        {
            ["Code"] = _site.Api.Sms.LastCode(mobile, OtpPurpose.Registration)!,
            ["DisplayName"] = "Test",
            ["Password"] = Password,
            ["ConfirmPassword"] = Password + "x",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("تکرار رمز عبور با رمز یکسان نیست", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Wrong_password_shows_the_api_message_in_the_page_language()
    {
        var response = await _site.Browser().SubmitAsync("/en/account/signin", "/en/account/signin", new Dictionary<string, string>
        {
            ["Mobile"] = ApiClientExtensions.NewMobile(),
            ["Password"] = "Wrong12345",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Mobile number or password is not correct.", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Admin_requires_the_staff_cookie_and_accepts_the_seeded_admin()
    {
        var browser = _site.Browser();

        var anonymous = await browser.GetAsync("/admin");
        Assert.Equal(HttpStatusCode.Redirect, anonymous.StatusCode);
        Assert.StartsWith("/admin/account/signin", anonymous.RedirectPath());

        var signIn = await browser.SubmitAsync("/admin/account/signin", "/admin/account/signin", new Dictionary<string, string>
        {
            ["Mobile"] = ApiFactory.AdminMobile,
            ["Password"] = ApiFactory.AdminPassword,
        });
        Assert.Equal(HttpStatusCode.Redirect, signIn.StatusCode);
        Assert.Equal("/admin", signIn.RedirectPath());

        var dashboard = await browser.GetPageAsync("/admin");
        Assert.Contains("پیشخوان", dashboard);
        Assert.Contains("Smoke Admin", dashboard);
    }

    [Fact]
    public async Task Member_account_is_refused_by_admin_sign_in()
    {
        var browser = _site.Browser();
        var mobile = ApiClientExtensions.NewMobile();
        await browser.SubmitAsync("/account/register", "/account/register", new Dictionary<string, string> { ["Mobile"] = mobile });
        await browser.SubmitAsync("/account/verify", "/account/verify", new Dictionary<string, string>
        {
            ["Code"] = _site.Api.Sms.LastCode(mobile, OtpPurpose.Registration)!,
            ["DisplayName"] = "Member",
            ["Password"] = Password,
            ["ConfirmPassword"] = Password,
        });

        var response = await browser.SubmitAsync("/admin/account/signin", "/admin/account/signin", new Dictionary<string, string>
        {
            ["Mobile"] = mobile,
            ["Password"] = Password,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("این حساب دسترسی به بخش مدیریت ندارد", await response.Content.ReadAsStringAsync());
        Assert.Equal(HttpStatusCode.Redirect, (await browser.GetAsync("/admin")).StatusCode);
    }

    [Fact]
    public async Task Sign_out_clears_the_member_session()
    {
        var browser = _site.Browser();
        var mobile = ApiClientExtensions.NewMobile();
        await browser.SubmitAsync("/account/register", "/account/register", new Dictionary<string, string> { ["Mobile"] = mobile });
        await browser.SubmitAsync("/account/verify", "/account/verify", new Dictionary<string, string>
        {
            ["Code"] = _site.Api.Sms.LastCode(mobile, OtpPurpose.Registration)!,
            ["DisplayName"] = "Leaving",
            ["Password"] = Password,
            ["ConfirmPassword"] = Password,
        });

        var signOut = await browser.SubmitAsync("/", "/account/signout", new Dictionary<string, string>());
        Assert.Equal(HttpStatusCode.Redirect, signOut.StatusCode);

        Assert.Equal(HttpStatusCode.Redirect, (await browser.GetAsync("/account")).StatusCode);
    }

    /// <summary>Any src/href pointing off-site (http://, https://, //) except our own absolute hreflang links.</summary>
    [GeneratedRegex("(?:src|href)=\"(?:https?:)?//(?!localhost)", RegexOptions.IgnoreCase)]
    private static partial Regex ExternalAsset();
}
