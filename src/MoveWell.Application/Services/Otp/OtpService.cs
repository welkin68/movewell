using CookieBytes.Core.Application.Abstractions;
using FluentResults;
using Microsoft.Extensions.Options;
using MoveWell.Application.Abstractions.Messaging;
using MoveWell.Application.Abstractions.Persistence;
using MoveWell.Application.Abstractions.Security;
using MoveWell.Application.Common.Errors;
using MoveWell.Application.Options;
using MoveWell.Domain.Aggregates.OtpChallenges;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Shared.Enumerations;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Application.Services.Otp;

/// <summary>Shared OTP steps of registration and password reset: send with limits, verify against the latest code.</summary>
public sealed class OtpService
{
    private readonly IMoveWellUnitOfWork _unitOfWork;
    private readonly IOtpCodeGenerator _codeGenerator;
    private readonly IOtpHasher _hasher;
    private readonly ISmsSender _smsSender;
    private readonly IDateTimeProvider _clock;
    private readonly OtpOptions _options;

    public OtpService(
        IMoveWellUnitOfWork unitOfWork,
        IOtpCodeGenerator codeGenerator,
        IOtpHasher hasher,
        ISmsSender smsSender,
        IDateTimeProvider clock,
        IOptions<OtpOptions> options)
    {
        _unitOfWork = unitOfWork;
        _codeGenerator = codeGenerator;
        _hasher = hasher;
        _smsSender = smsSender;
        _clock = clock;
        _options = options.Value;
    }

    /// <summary>Applies resend/hourly limits, stores a new hashed challenge and sends the SMS.</summary>
    public async Task<Result> SendAsync(MobileNumber mobile, OtpPurpose purpose, Culture culture, CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;

        var latest = await _unitOfWork.OtpChallenges.GetLatestUnconsumedAsync(mobile, purpose, cancellationToken);
        if (latest is not null)
        {
            var wait = latest.CreatedAt.AddSeconds(_options.ResendIntervalSeconds) - now;
            if (wait > TimeSpan.Zero)
            {
                return Result.Fail(AuthErrors.ResendTooSoon((int)Math.Ceiling(wait.TotalSeconds)));
            }
        }

        var sentLastHour = await _unitOfWork.OtpChallenges.CountIssuedSinceAsync(mobile, now.AddHours(-1), cancellationToken);
        if (sentLastHour >= _options.MaxPerHourPerMobile)
        {
            return Result.Fail(AuthErrors.TooManyCodes);
        }

        var code = _codeGenerator.Generate(_options.CodeLength);
        var challenge = OtpChallenge.Issue(
            mobile,
            purpose,
            _hasher.Hash(code, mobile, purpose),
            now,
            TimeSpan.FromSeconds(_options.LifetimeSeconds),
            _options.MaxAttempts);
        if (challenge.IsFailed)
        {
            return challenge.ToResult();
        }

        await _unitOfWork.OtpChallenges.AddAsync(challenge.Value, cancellationToken);
        await _unitOfWork.SaveAndDispatchEventsAsync(cancellationToken);

        var sent = await _smsSender.SendOtpAsync(mobile, code, purpose, culture, cancellationToken);
        return sent.IsSuccess ? Result.Ok() : Result.Fail(AuthErrors.SmsFailed);
    }

    /// <summary>
    /// Verifies <paramref name="code"/> against the latest open challenge. The attempt is saved even on failure.
    /// On success the challenge is returned (verified, not yet consumed) so the caller consumes it after its action succeeds.
    /// </summary>
    public async Task<Result<OtpChallenge>> VerifyAsync(MobileNumber mobile, OtpPurpose purpose, string code, CancellationToken cancellationToken)
    {
        var challenge = await _unitOfWork.OtpChallenges.GetLatestUnconsumedAsync(mobile, purpose, cancellationToken);
        if (challenge is null)
        {
            return Result.Fail(AuthErrors.CodeNotFound);
        }

        var verified = challenge.Verify(_hasher.Hash(code, mobile, purpose), _clock.UtcNow);
        if (verified.IsFailed)
        {
            await _unitOfWork.SaveAsync(cancellationToken);
            return verified;
        }

        return Result.Ok(challenge);
    }
}
