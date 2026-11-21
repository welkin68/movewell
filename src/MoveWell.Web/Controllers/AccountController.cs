using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MoveWell.Common.Contracts.Auth;
using MoveWell.Web.Authentication;
using MoveWell.Web.Localization;
using MoveWell.Web.Models.Account;
using MoveWell.Web.Models.Shared;
using MoveWell.Web.Services.Api;
using MoveWell.Web.Services.Auth;

namespace MoveWell.Web.Controllers;

/// <summary>
/// Public account pages (BFF, D-16). Every rule is the Api's: this controller only moves input to the Api,
/// keeps the in-progress mobile number in TempData (never in the URL), and sets/clears the member cookie.
/// </summary>
[Route("account")]
public sealed class AccountController : Controller
{
    private const string PendingRegistrationKey = "PendingRegistration";
    private const string PendingResetKey = "PendingReset";
    private const string FlashKey = "Flash";
    private const int ResendWaitSeconds = 120;

    private readonly IAuthApiClient _authApi;
    private readonly IStringLocalizer _text;

    public AccountController(IAuthApiClient authApi, UiText ui)
    {
        _authApi = authApi;
        _text = ui.For("public/account/account");
    }

    // ---------- registration ----------

    [HttpGet("register")]
    public IActionResult Register() => SignedInMember() ? RedirectToHome() : View(new RegisterForm());

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterForm form, CancellationToken cancellationToken)
    {
        var result = await _authApi.StartRegistrationAsync(form.Mobile, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(form.Errors, result);
            return View(form);
        }

        TempData[PendingRegistrationKey] = form.Mobile;
        return RedirectToAction(nameof(Verify));
    }

    [HttpGet("verify")]
    public IActionResult Verify()
    {
        if (PeekPending(PendingRegistrationKey) is not { } mobile)
        {
            return RedirectToAction(nameof(Register));
        }

        return View(new VerifyForm { MaskedMobile = Mask(mobile), ResendWaitSeconds = ResendWaitSeconds });
    }

    [HttpPost("verify")]
    public async Task<IActionResult> Verify(VerifyForm form, CancellationToken cancellationToken)
    {
        if (PeekPending(PendingRegistrationKey) is not { } mobile)
        {
            return RedirectToAction(nameof(Register));
        }

        form.MaskedMobile = Mask(mobile);
        if (form.Password != form.ConfirmPassword)
        {
            form.Errors.Add(_text["password.mismatch"]);
            return View(form);
        }

        var result = await _authApi.CompleteRegistrationAsync(
            new CompleteRegistrationRequest(mobile, form.Code, form.Password, form.DisplayName),
            cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            AddErrors(form.Errors, result);
            return View(form);
        }

        TempData.Remove(PendingRegistrationKey);
        await SessionCookie.SignInAsync(HttpContext, WebAuthSchemes.Member, result.Value);
        TempData[FlashKey] = "flash.welcome";
        return RedirectToHome();
    }

    [HttpPost("verify/resend")]
    public async Task<IActionResult> ResendRegistrationCode(CancellationToken cancellationToken)
    {
        if (PeekPending(PendingRegistrationKey) is not { } mobile)
        {
            return RedirectToAction(nameof(Register));
        }

        var result = await _authApi.StartRegistrationAsync(mobile, cancellationToken);
        if (!result.Succeeded)
        {
            var form = new VerifyForm { MaskedMobile = Mask(mobile), ResendWaitSeconds = RetryAfterSeconds(result) };
            AddErrors(form.Errors, result);
            return View(nameof(Verify), form);
        }

        TempData[FlashKey] = "flash.code_sent";
        return RedirectToAction(nameof(Verify));
    }

    // ---------- sign in / out ----------

    [HttpGet("signin")]
    public IActionResult SignIn(string? returnUrl = null) =>
        SignedInMember() ? RedirectToLocal(returnUrl) : View(new SignInForm { ReturnUrl = returnUrl });

    [HttpPost("signin")]
    public async Task<IActionResult> SignIn(SignInForm form, CancellationToken cancellationToken)
    {
        var result = await _authApi.SignInAsync(form.Mobile, form.Password, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            AddErrors(form.Errors, result);
            form.Password = string.Empty;
            return View(form);
        }

        await SessionCookie.SignInAsync(HttpContext, WebAuthSchemes.Member, result.Value);
        TempData[FlashKey] = "flash.signed_in";
        return RedirectToLocal(form.ReturnUrl);
    }

    [HttpPost("signout")]
    public async Task<IActionResult> SignOut(CancellationToken cancellationToken)
    {
        var auth = await HttpContext.AuthenticateAsync(WebAuthSchemes.Member);
        var refreshToken = auth.Properties?.GetTokenValue(SessionCookie.RefreshTokenName);
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _authApi.SignOutAsync(refreshToken, cancellationToken); // best effort: the cookie goes either way
        }

        await HttpContext.SignOutAsync(WebAuthSchemes.Member);
        TempData[FlashKey] = "flash.signed_out";
        return RedirectToHome();
    }

    // ---------- account page ----------

    [HttpGet("")]
    [Authorize(AuthenticationSchemes = WebAuthSchemes.Member)]
    public IActionResult Index() => View();

    // ---------- password reset ----------

    [HttpGet("forgot")]
    public IActionResult Forgot() => View(new ForgotForm());

    [HttpPost("forgot")]
    public async Task<IActionResult> Forgot(ForgotForm form, CancellationToken cancellationToken)
    {
        var result = await _authApi.StartPasswordResetAsync(form.Mobile, cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(form.Errors, result);
            return View(form);
        }

        TempData[PendingResetKey] = form.Mobile;
        return RedirectToAction(nameof(Reset));
    }

    [HttpGet("reset")]
    public IActionResult Reset() =>
        PeekPending(PendingResetKey) is { } mobile
            ? View(new ResetForm { MaskedMobile = Mask(mobile) })
            : RedirectToAction(nameof(Forgot));

    [HttpPost("reset")]
    public async Task<IActionResult> Reset(ResetForm form, CancellationToken cancellationToken)
    {
        if (PeekPending(PendingResetKey) is not { } mobile)
        {
            return RedirectToAction(nameof(Forgot));
        }

        form.MaskedMobile = Mask(mobile);
        if (form.NewPassword != form.ConfirmPassword)
        {
            form.Errors.Add(_text["password.mismatch"]);
            return View(form);
        }

        var result = await _authApi.CompletePasswordResetAsync(
            new CompletePasswordResetRequest(mobile, form.Code, form.NewPassword),
            cancellationToken);
        if (!result.Succeeded)
        {
            AddErrors(form.Errors, result);
            return View(form);
        }

        TempData.Remove(PendingResetKey);
        await HttpContext.SignOutAsync(WebAuthSchemes.Member); // the Api revoked every session of this user
        TempData[FlashKey] = "flash.password_reset";
        return RedirectToAction(nameof(SignIn));
    }

    // ---------- helpers ----------

    private bool SignedInMember() => User.Identity?.IsAuthenticated == true;

    private string? PeekPending(string key)
    {
        var value = TempData.Peek(key) as string;
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static void AddErrors(FormErrors errors, ApiResult result) =>
        errors.AddRange(result.Errors.Select(error => error.Message));

    private static int RetryAfterSeconds(ApiResult result) =>
        result.Status == System.Net.HttpStatusCode.TooManyRequests ? ResendWaitSeconds : 0;

    /// <summary>Same masking as the Api (0912***6789); works on any input format because only digits are kept.</summary>
    private static string Mask(string mobile)
    {
        var digits = new string(mobile.Where(char.IsDigit).Select(ch => (char)('0' + (int)char.GetNumericValue(ch))).ToArray());
        // LRI … PDI keeps "0912***6789" in reading order inside Persian (RTL) sentences.
        return digits.Length >= 8 ? $"⁦{digits[..4]}***{digits[^4..]}⁩" : "***";
    }

    private IActionResult RedirectToHome() => RedirectToAction("Index", "Home");

    private IActionResult RedirectToLocal(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToHome();
}
