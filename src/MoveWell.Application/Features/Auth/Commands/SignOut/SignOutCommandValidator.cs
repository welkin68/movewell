using FluentResults;
using FluentValidation;
using MediatR;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Common.Errors;

namespace MoveWell.Application.Features.Auth.Commands.SignOut;

public sealed class SignOutCommandValidator : AbstractValidator<SignOutCommand>
{
    public SignOutCommandValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty().WithMessage(ValidationCodes.RefreshTokenRequired);
    }
}
