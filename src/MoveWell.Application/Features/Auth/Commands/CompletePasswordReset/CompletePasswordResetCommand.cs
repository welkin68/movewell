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

/// <summary>Verifies the reset OTP, sets the new password and signs the user out everywhere.</summary>
public sealed record CompletePasswordResetCommand(string Mobile, string Code, string NewPassword) : IRequest<Result>;
