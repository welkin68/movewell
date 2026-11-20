using FluentValidation;
using MoveWell.Application.Features.Auth.Commands.Shared;

namespace MoveWell.Application.Features.Auth.Commands.CompleteRegistration;

public sealed class CompleteRegistrationCommandValidator : AbstractValidator<CompleteRegistrationCommand>
{
    public CompleteRegistrationCommandValidator()
    {
        RuleFor(command => command.Mobile).ValidMobileInput();
        RuleFor(command => command.Code).ValidOtpCode();
        RuleFor(command => command.Password).ValidPasswordInput();
        RuleFor(command => command.DisplayName).ValidDisplayName();
    }
}
