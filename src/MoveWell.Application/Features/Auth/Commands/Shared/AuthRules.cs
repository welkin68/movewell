using FluentValidation;
using MoveWell.Application.Common.Errors;
using MoveWell.Domain.Aggregates.UserProfiles;

namespace MoveWell.Application.Features.Auth.Commands.Shared;

/// <summary>Input-shape rules reused by the auth validators. Business rules stay in the domain / Identity policy.</summary>
public static class AuthRules
{
    public const int PasswordMaxLength = 128;

    public static IRuleBuilderOptions<T, string> ValidMobileInput<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage(ValidationCodes.MobileRequired);

    public static IRuleBuilderOptions<T, string> ValidPasswordInput<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage(ValidationCodes.PasswordRequired)
            .MaximumLength(PasswordMaxLength).WithMessage(ValidationCodes.PasswordTooLong);

    public static IRuleBuilderOptions<T, string> ValidOtpCode<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage(ValidationCodes.CodeFormat)
            .Matches("^[0-9]{4,8}$").WithMessage(ValidationCodes.CodeFormat);

    public static IRuleBuilderOptions<T, string> ValidDisplayName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().WithMessage(ValidationCodes.DisplayNameLength)
            .Must(name => name.Trim().Length is >= UserProfile.DisplayNameMinLength and <= UserProfile.DisplayNameMaxLength)
            .WithMessage(ValidationCodes.DisplayNameLength);
}
