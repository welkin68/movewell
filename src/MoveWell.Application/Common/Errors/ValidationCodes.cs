namespace MoveWell.Application.Common.Errors;

/// <summary>
/// FluentValidation messages in MoveWell ARE error codes (Core turns each failure into an Error whose message is this text);
/// the Api localizes them like domain error codes.
/// </summary>
public static class ValidationCodes
{
    public const string MobileRequired = "validation.mobile_required";
    public const string PasswordRequired = "validation.password_required";
    public const string PasswordTooLong = "validation.password_too_long";
    public const string CodeFormat = "validation.code_format";
    public const string DisplayNameLength = "validation.display_name_length";
    public const string RefreshTokenRequired = "validation.refresh_token_required";
}
