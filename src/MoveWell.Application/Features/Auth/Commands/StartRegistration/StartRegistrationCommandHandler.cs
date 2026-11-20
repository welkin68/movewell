using FluentResults;
using MediatR;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Common.Errors;
using MoveWell.Application.Services.Otp;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Shared.Enumerations;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Application.Features.Auth.Commands.StartRegistration;

/// <remarks>
/// Registration says "already registered" (409) on purpose: hiding it would leave the person waiting for an SMS that never comes.
/// Mass probing is limited by per-IP rate limits on the endpoint. Password reset stays neutral.
/// </remarks>
public sealed class StartRegistrationCommandHandler : IRequestHandler<StartRegistrationCommand, Result>
{
    private readonly IIdentityService _identity;
    private readonly OtpService _otp;

    public StartRegistrationCommandHandler(IIdentityService identity, OtpService otp)
    {
        _identity = identity;
        _otp = otp;
    }

    public async Task<Result> Handle(StartRegistrationCommand command, CancellationToken cancellationToken)
    {
        var mobile = MobileNumber.Create(command.Mobile);
        if (mobile.IsFailed)
        {
            return mobile.ToResult();
        }

        if (await _identity.MobileExistsAsync(mobile.Value, cancellationToken))
        {
            return Result.Fail(AuthErrors.AlreadyRegistered);
        }

        return await _otp.SendAsync(mobile.Value, OtpPurpose.Registration, Culture.FromCode(command.Culture), cancellationToken);
    }
}
