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

public sealed class StartPasswordResetCommandHandler : IRequestHandler<StartPasswordResetCommand, Result>
{
    private readonly IIdentityService _identity;
    private readonly OtpService _otp;

    public StartPasswordResetCommandHandler(IIdentityService identity, OtpService otp)
    {
        _identity = identity;
        _otp = otp;
    }

    public async Task<Result> Handle(StartPasswordResetCommand command, CancellationToken cancellationToken)
    {
        var mobile = MobileNumber.Create(command.Mobile);
        if (mobile.IsFailed)
        {
            return mobile.ToResult();
        }

        if (!await _identity.MobileExistsAsync(mobile.Value, cancellationToken))
        {
            return Result.Ok();
        }

        return await _otp.SendAsync(mobile.Value, OtpPurpose.PasswordReset, Culture.FromCode(command.Culture), cancellationToken);
    }
}
