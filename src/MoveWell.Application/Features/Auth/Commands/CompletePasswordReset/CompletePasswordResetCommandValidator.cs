using CookieBytes.Core.Application.Abstractions;
using FluentResults;
using FluentValidation;
using MediatR;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Abstractions.Persistence;
using MoveWell.Application.Common.Errors;
using MoveWell.Application.Features.Auth.Commands.Shared;
using MoveWell.Application.Services.Otp;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Application.Features.Auth.Commands.CompletePasswordReset;

public sealed class CompletePasswordResetCommandValidator : AbstractValidator<CompletePasswordResetCommand>
{
    public CompletePasswordResetCommandValidator()
    {
        RuleFor(command => command.Mobile).ValidMobileInput();
        RuleFor(command => command.Code).ValidOtpCode();
        RuleFor(command => command.NewPassword).ValidPasswordInput();
    }
}
