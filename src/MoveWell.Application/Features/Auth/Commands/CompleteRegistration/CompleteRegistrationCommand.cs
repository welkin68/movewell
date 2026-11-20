using FluentResults;
using MediatR;
using MoveWell.Application.DTOs.Auth;

namespace MoveWell.Application.Features.Auth.Commands.CompleteRegistration;

/// <summary>Step 2: verify the OTP, create the Member account + profile, and sign in.</summary>
public sealed record CompleteRegistrationCommand(
    string Mobile,
    string Code,
    string Password,
    string DisplayName,
    string Culture,
    string? ClientIp) : IRequest<Result<AuthSessionDto>>;
