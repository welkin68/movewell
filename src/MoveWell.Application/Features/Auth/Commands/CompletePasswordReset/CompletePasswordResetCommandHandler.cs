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

public sealed class CompletePasswordResetCommandHandler : IRequestHandler<CompletePasswordResetCommand, Result>
{
    private readonly IMoveWellUnitOfWork _unitOfWork;
    private readonly IIdentityService _identity;
    private readonly ITokenService _tokens;
    private readonly OtpService _otp;
    private readonly IDateTimeProvider _clock;

    public CompletePasswordResetCommandHandler(
        IMoveWellUnitOfWork unitOfWork,
        IIdentityService identity,
        ITokenService tokens,
        OtpService otp,
        IDateTimeProvider clock)
    {
        _unitOfWork = unitOfWork;
        _identity = identity;
        _tokens = tokens;
        _otp = otp;
        _clock = clock;
    }

    public async Task<Result> Handle(CompletePasswordResetCommand command, CancellationToken cancellationToken)
    {
        var mobile = MobileNumber.Create(command.Mobile);
        if (mobile.IsFailed)
        {
            return mobile.ToResult();
        }

        var password = await _identity.ValidatePasswordAsync(command.NewPassword, cancellationToken);
        if (password.IsFailed)
        {
            return password;
        }

        // Unknown numbers have no reset challenge, so they fail exactly like a wrong code.
        var challenge = await _otp.VerifyAsync(mobile.Value, OtpPurpose.PasswordReset, command.Code, cancellationToken);
        if (challenge.IsFailed)
        {
            return challenge.ToResult();
        }

        var user = await _identity.FindByMobileAsync(mobile.Value, cancellationToken);
        if (user is null)
        {
            return Result.Fail(AuthErrors.CodeNotFound);
        }

        var reset = await _identity.ResetPasswordAsync(user.UserId, command.NewPassword, cancellationToken);
        if (reset.IsFailed)
        {
            return reset;
        }

        challenge.Value.Consume(_clock.UtcNow);
        await _unitOfWork.SaveAndDispatchEventsAsync(cancellationToken);
        await _tokens.RevokeAllAsync(user.UserId, cancellationToken);

        return Result.Ok();
    }
}
