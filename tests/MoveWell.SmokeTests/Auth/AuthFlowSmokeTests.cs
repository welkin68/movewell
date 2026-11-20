using System.Net;
using System.Net.Http.Json;
using MoveWell.Common.Contracts.Auth;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.SmokeTests.Infrastructure;

namespace MoveWell.SmokeTests.Auth;

/// <summary>End-to-end auth flows through HTTP → MediatR → EF → SQL Server (LocalDB).</summary>
public sealed class AuthFlowSmokeTests : IClassFixture<ApiFactory>
{
    private const string Password = "Movewell123";
    private readonly ApiFactory _factory;

    public AuthFlowSmokeTests(ApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Health_ready_reports_database_up()
    {
        var response = await _factory.CreateClient().GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_then_sign_in_returns_member_session()
    {
        var client = _factory.CreateClient();
        var mobile = ApiClientExtensions.NewMobile();

        var session = await RegisterAsync(client, mobile, "سارا احمدی");

        Assert.False(string.IsNullOrWhiteSpace(session.AccessToken));
        Assert.Equal(mobile, session.User.Mobile);
        Assert.Equal("سارا احمدی", session.User.DisplayName);
        Assert.Contains("Member", session.User.Roles);

        var signIn = await client.PostAsync("/api/v1/auth/signin", new SignInRequest(mobile, Password));
        Assert.Equal(HttpStatusCode.OK, signIn.StatusCode);
    }

    [Fact]
    public async Task Mobile_formats_are_normalized_across_flows()
    {
        var client = _factory.CreateClient();
        var mobile = ApiClientExtensions.NewMobile();
        var international = "+98" + mobile[1..];
        var persianDigits = string.Concat(mobile.Select(ch => (char)('۰' + (ch - '0'))));

        await RegisterAsync(client, international, "Ali");

        var signIn = await client.PostAsync("/api/v1/auth/signin", new SignInRequest(persianDigits, Password));
        Assert.Equal(HttpStatusCode.OK, signIn.StatusCode);
    }

    [Fact]
    public async Task Registering_an_existing_mobile_returns_conflict_in_persian()
    {
        var client = _factory.CreateClient().WithLanguage("fa-IR");
        var mobile = ApiClientExtensions.NewMobile();
        await RegisterAsync(client, mobile, "Reza");

        var response = await client.PostAsync("/api/v1/auth/register/start", new StartRegistrationRequest(mobile));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var error = Assert.Single(await response.ReadErrorsAsync());
        Assert.Equal("auth.already_registered", error.Code);
        Assert.Contains("ثبت‌نام", error.Message);
    }

    [Fact]
    public async Task Wrong_code_is_rejected_but_does_not_block_the_right_one()
    {
        var client = _factory.CreateClient();
        var mobile = ApiClientExtensions.NewMobile();
        await StartRegistrationAsync(client, mobile);
        var code = _factory.Sms.LastCode(mobile, OtpPurpose.Registration)!;
        var wrong = code == "000000" ? "111111" : "000000";

        var bad = await client.PostAsync("/api/v1/auth/register/complete", new CompleteRegistrationRequest(mobile, wrong, Password, "Nima"));
        Assert.Equal(HttpStatusCode.BadRequest, bad.StatusCode);
        Assert.Equal("otp.invalid", (await bad.ReadErrorsAsync())[0].Code);

        var good = await client.PostAsync("/api/v1/auth/register/complete", new CompleteRegistrationRequest(mobile, code, Password, "Nima"));
        Assert.Equal(HttpStatusCode.OK, good.StatusCode);
    }

    [Fact]
    public async Task Weak_password_is_rejected_before_the_code_is_used()
    {
        var client = _factory.CreateClient();
        var mobile = ApiClientExtensions.NewMobile();
        await StartRegistrationAsync(client, mobile);
        var code = _factory.Sms.LastCode(mobile, OtpPurpose.Registration)!;

        var weak = await client.PostAsync("/api/v1/auth/register/complete", new CompleteRegistrationRequest(mobile, code, "short", "Mina"));
        Assert.Equal(HttpStatusCode.BadRequest, weak.StatusCode);
        Assert.Contains(await weak.ReadErrorsAsync(), error => error.Code == "password.too_short");

        var ok = await client.PostAsync("/api/v1/auth/register/complete", new CompleteRegistrationRequest(mobile, code, Password, "Mina"));
        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
    }

    [Fact]
    public async Task Requesting_a_second_code_too_soon_returns_429_with_retry_after()
    {
        var client = _factory.CreateClient();
        var mobile = ApiClientExtensions.NewMobile();
        await StartRegistrationAsync(client, mobile);

        var again = await client.PostAsync("/api/v1/auth/register/start", new StartRegistrationRequest(mobile));

        Assert.Equal(HttpStatusCode.TooManyRequests, again.StatusCode);
        Assert.Equal("otp.resend_too_soon", (await again.ReadErrorsAsync())[0].Code);
        Assert.NotNull(again.Headers.RetryAfter);
    }

    [Fact]
    public async Task Wrong_password_returns_401_with_english_message()
    {
        var client = _factory.CreateClient().WithLanguage("en-US");
        var mobile = ApiClientExtensions.NewMobile();
        await RegisterAsync(client, mobile, "Sara");

        var response = await client.PostAsync("/api/v1/auth/signin", new SignInRequest(mobile, "Wrong12345"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var error = Assert.Single(await response.ReadErrorsAsync());
        Assert.Equal("auth.invalid_credentials", error.Code);
        Assert.Equal("Mobile number or password is not correct.", error.Message);
    }

    [Fact]
    public async Task Unknown_mobile_gets_the_same_answer_as_wrong_password()
    {
        var response = await _factory.CreateClient()
            .PostAsync("/api/v1/auth/signin", new SignInRequest(ApiClientExtensions.NewMobile(), Password));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("auth.invalid_credentials", (await response.ReadErrorsAsync())[0].Code);
    }

    [Fact]
    public async Task Refresh_rotates_and_reusing_an_old_token_revokes_the_family()
    {
        var client = _factory.CreateClient();
        var session = await RegisterAsync(client, ApiClientExtensions.NewMobile(), "Omid");

        var first = await client.PostAsync("/api/v1/auth/refresh", new RefreshSessionRequest(session.RefreshToken));
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var rotated = (await first.Content.ReadFromJsonAsync<AuthSessionResponse>())!;
        Assert.NotEqual(session.RefreshToken, rotated.RefreshToken);

        var reuse = await client.PostAsync("/api/v1/auth/refresh", new RefreshSessionRequest(session.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);

        var afterTheft = await client.PostAsync("/api/v1/auth/refresh", new RefreshSessionRequest(rotated.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, afterTheft.StatusCode);
    }

    [Fact]
    public async Task Sign_out_revokes_the_refresh_token()
    {
        var client = _factory.CreateClient();
        var session = await RegisterAsync(client, ApiClientExtensions.NewMobile(), "Leila");

        var signOut = await client.PostAsync("/api/v1/auth/signout", new SignOutRequest(session.RefreshToken));
        Assert.Equal(HttpStatusCode.NoContent, signOut.StatusCode);

        var refresh = await client.PostAsync("/api/v1/auth/refresh", new RefreshSessionRequest(session.RefreshToken));
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task Password_reset_with_otp_changes_password_and_ends_sessions()
    {
        var client = _factory.CreateClient();
        var mobile = ApiClientExtensions.NewMobile();
        var session = await RegisterAsync(client, mobile, "Hamid");

        var start = await client.PostAsync("/api/v1/auth/password/reset/start", new StartPasswordResetRequest(mobile));
        Assert.Equal(HttpStatusCode.NoContent, start.StatusCode);
        var code = _factory.Sms.LastCode(mobile, OtpPurpose.PasswordReset)!;

        var complete = await client.PostAsync(
            "/api/v1/auth/password/reset/complete",
            new CompletePasswordResetRequest(mobile, code, "NewPassword9"));
        Assert.Equal(HttpStatusCode.NoContent, complete.StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsync("/api/v1/auth/signin", new SignInRequest(mobile, Password))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync("/api/v1/auth/signin", new SignInRequest(mobile, "NewPassword9"))).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await client.PostAsync("/api/v1/auth/refresh", new RefreshSessionRequest(session.RefreshToken))).StatusCode);
    }

    [Fact]
    public async Task Password_reset_for_unknown_mobile_answers_ok_and_sends_nothing()
    {
        var mobile = ApiClientExtensions.NewMobile();

        var response = await _factory.CreateClient()
            .PostAsync("/api/v1/auth/password/reset/start", new StartPasswordResetRequest(mobile));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Null(_factory.Sms.LastCode(mobile, OtpPurpose.PasswordReset));
    }

    [Fact]
    public async Task Change_password_requires_a_token_and_works_with_one()
    {
        var anonymous = await _factory.CreateClient()
            .PostAsync("/api/v1/auth/password/change", new ChangePasswordRequest(Password, "Changed12345"));
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);

        var client = _factory.CreateClient();
        var mobile = ApiClientExtensions.NewMobile();
        var session = await RegisterAsync(client, mobile, "Kian");

        var changed = await client.WithBearer(session.AccessToken)
            .PostAsync("/api/v1/auth/password/change", new ChangePasswordRequest(Password, "Changed12345"));
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);

        var signIn = await _factory.CreateClient().PostAsync("/api/v1/auth/signin", new SignInRequest(mobile, "Changed12345"));
        Assert.Equal(HttpStatusCode.OK, signIn.StatusCode);
    }

    private async Task StartRegistrationAsync(HttpClient client, string mobile)
    {
        var start = await client.PostAsync("/api/v1/auth/register/start", new StartRegistrationRequest(mobile));
        Assert.Equal(HttpStatusCode.NoContent, start.StatusCode);
    }

    private async Task<AuthSessionResponse> RegisterAsync(HttpClient client, string mobile, string displayName)
    {
        await StartRegistrationAsync(client, mobile);
        var code = _factory.Sms.LastCode(mobile, OtpPurpose.Registration)
            ?? throw new InvalidOperationException("No SMS captured.");

        var complete = await client.PostAsync(
            "/api/v1/auth/register/complete",
            new CompleteRegistrationRequest(mobile, code, Password, displayName));
        Assert.Equal(HttpStatusCode.OK, complete.StatusCode);

        return (await complete.Content.ReadFromJsonAsync<AuthSessionResponse>())!;
    }
}
