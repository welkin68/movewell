using CookieBytes.Core.Application.Abstractions;
using FluentResults;
using FluentValidation;
using MediatR;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Common.Errors;
using MoveWell.Application.Features.Auth.Commands.Shared;

namespace MoveWell.Application.Features.Auth.Commands.ChangePassword;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(command => command.CurrentPassword).ValidPasswordInput();
        RuleFor(command => command.NewPassword).ValidPasswordInput();
    }
}
