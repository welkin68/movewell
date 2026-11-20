using FluentValidation;
using MoveWell.Application.Common.Errors;

namespace MoveWell.Application.Features.Auth.Commands.StartRegistration;

public sealed class StartRegistrationCommandValidator : AbstractValidator<StartRegistrationCommand>
{
    public StartRegistrationCommandValidator()
    {
        RuleFor(command => command.Mobile).NotEmpty().WithMessage(ValidationCodes.MobileRequired);
    }
}
