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

/// <summary>Sends a reset OTP if the account exists. Always answers OK for unknown numbers (no account enumeration).</summary>
public sealed record StartPasswordResetCommand(string Mobile, string Culture) : IRequest<Result>;
