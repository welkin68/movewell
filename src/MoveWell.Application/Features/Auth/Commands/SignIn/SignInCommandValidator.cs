using FluentValidation;
using MoveWell.Application.Features.Auth.Commands.Shared;

namespace MoveWell.Application.Features.Auth.Commands.SignIn;

public sealed class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(command => command.Mobile).ValidMobileInput();
        RuleFor(command => command.Password).ValidPasswordInput();
    }
}
