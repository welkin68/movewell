using FluentResults;
using FluentValidation;
using MediatR;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Features.Auth.Commands.Shared;
using MoveWell.Application.Services.Otp;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Shared.Enumerations;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Application.Features.Auth.Commands.StartPasswordReset;

public sealed class StartPasswordResetCommandValidator : AbstractValidator<StartPasswordResetCommand>
{
    public StartPasswordResetCommandValidator()
    {
        RuleFor(command => command.Mobile).ValidMobileInput();
    }
}
