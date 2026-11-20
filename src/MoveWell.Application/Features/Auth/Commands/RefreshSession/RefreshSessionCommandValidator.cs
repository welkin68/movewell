using FluentResults;
using FluentValidation;
using MediatR;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Common.Errors;
using MoveWell.Application.DTOs.Auth;

namespace MoveWell.Application.Features.Auth.Commands.RefreshSession;

public sealed class RefreshSessionCommandValidator : AbstractValidator<RefreshSessionCommand>
{
    public RefreshSessionCommandValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty().WithMessage(ValidationCodes.RefreshTokenRequired);
    }
}
