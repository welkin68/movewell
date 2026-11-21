using MoveWell.Web.Models.Shared;

namespace MoveWell.Web.Models.Account;

// Form models of the public account pages. Validation rules live in the Api/domain; these only carry input.

public sealed class RegisterForm
{
    public string Mobile { get; set; } = string.Empty;

    public FormErrors Errors { get; } = new();
}

public sealed class VerifyForm
{
    public string Code { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;

    /// <summary>Shown masked, e.g. 0912***6789. Never posted back: the real number stays in TempData (no PII in URLs or forms).</summary>
    public string MaskedMobile { get; set; } = string.Empty;

    public int ResendWaitSeconds { get; set; }

    public FormErrors Errors { get; } = new();
}

public sealed class SignInForm
{
    public string Mobile { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }

    public FormErrors Errors { get; } = new();
}

public sealed class ForgotForm
{
    public string Mobile { get; set; } = string.Empty;

    public FormErrors Errors { get; } = new();
}

public sealed class ResetForm
{
    public string Code { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;

    public string ConfirmPassword { get; set; } = string.Empty;

    public string MaskedMobile { get; set; } = string.Empty;

    public FormErrors Errors { get; } = new();
}
