using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MoveWell.Common.Identity;
using MoveWell.Web.Authentication;
using MoveWell.Web.Localization;
using MoveWell.Web.Models.Account;
using MoveWell.Web.Services.Auth;

namespace MoveWell.Web.Areas.Admin.Controllers;

/// <summary>Staff sign-in for /admin: same Api sign-in as members, but only Admin/Content accounts get the staff cookie.</summary>
[Area("Admin")]
public sealed class AccountController : Controller
{
    private readonly IAuthApiClient _authApi;
    private readonly IStringLocalizer _text;

    public AccountController(IAuthApiClient authApi, UiText ui)
    {
        _authApi = authApi;
        _text = ui.For("admin/shared/layout");
    }

    [HttpGet]
    public IActionResult SignIn(string? returnUrl = null) => View(new SignInForm { ReturnUrl = returnUrl });

    [HttpPost]
    public async Task<IActionResult> SignIn(SignInForm form, CancellationToken cancellationToken)
    {
        var result = await _authApi.SignInAsync(form.Mobile, form.Password, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            form.Errors.AddRange(result.Errors.Select(error => error.Message));
            form.Password = string.Empty;
            return View(form);
        }

        var roles = result.Value.User.Roles;
        if (!roles.Contains(Roles.Admin) && !roles.Contains(Roles.Content))
        {
            await _authApi.SignOutAsync(result.Value.RefreshToken, cancellationToken); // do not leave a live session behind
            form.Errors.Add(_text["signin.not_staff"]);
            form.Password = string.Empty;
            return View(form);
        }

        await SessionCookie.SignInAsync(HttpContext, WebAuthSchemes.Staff, result.Value);
        return !string.IsNullOrEmpty(form.ReturnUrl) && Url.IsLocalUrl(form.ReturnUrl)
            ? LocalRedirect(form.ReturnUrl)
            : RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    public async Task<IActionResult> SignOut(CancellationToken cancellationToken)
    {
        var auth = await HttpContext.AuthenticateAsync(WebAuthSchemes.Staff);
        var refreshToken = auth.Properties?.GetTokenValue(SessionCookie.RefreshTokenName);
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _authApi.SignOutAsync(refreshToken, cancellationToken);
        }

        await HttpContext.SignOutAsync(WebAuthSchemes.Staff);
        return RedirectToAction(nameof(SignIn));
    }
}
