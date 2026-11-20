using FluentResults;
using MediatR;

namespace MoveWell.Application.Features.Auth.Commands.StartRegistration;

/// <summary>Step 1 of registration: send an OTP to a mobile that has no account yet (D-13).</summary>
public sealed record StartRegistrationCommand(string Mobile, string Culture) : IRequest<Result>;
