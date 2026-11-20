using System.Security.Cryptography;
using System.Text;
using CookieBytes.Core.Domain.SeedWork;
using FluentResults;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Shared.Errors;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Domain.Aggregates.OtpChallenges;

/// <summary>
/// One SMS verification code sent to a mobile for one purpose (registration or password reset, D-13).
/// Only a hash of the code is stored. Lifecycle: Issued, then Verified, then Consumed; or Expired / locked by attempts.
/// </summary>
/// <remarks>
/// <see cref="Verify"/> changes state even when it fails (it counts the attempt), so callers must save after a failed verify too.
/// </remarks>
public class OtpChallenge : AggregateRoot
{
    protected OtpChallenge()
    {
    }

    private OtpChallenge(
        MobileNumber mobile,
        OtpPurpose purpose,
        string codeHash,
        DateTime expiresAt,
        int maxAttempts)
    {
        Mobile = mobile;
        Purpose = purpose;
        CodeHash = codeHash;
        ExpiresAt = expiresAt;
        MaxAttempts = maxAttempts;
    }

    public MobileNumber Mobile { get; private set; } = null!;

    public OtpPurpose Purpose { get; private set; } = null!;

    public string CodeHash { get; private set; } = string.Empty;

    public DateTime ExpiresAt { get; private set; }

    public int Attempts { get; private set; }

    public int MaxAttempts { get; private set; }

    public DateTime? VerifiedAt { get; private set; }

    public DateTime? ConsumedAt { get; private set; }

    public static Result<OtpChallenge> Issue(
        MobileNumber mobile,
        OtpPurpose purpose,
        string codeHash,
        DateTime utcNow,
        TimeSpan lifetime,
        int maxAttempts)
    {
        ArgumentNullException.ThrowIfNull(mobile);
        ArgumentNullException.ThrowIfNull(purpose);
        ArgumentException.ThrowIfNullOrWhiteSpace(codeHash);

        if (lifetime <= TimeSpan.Zero || maxAttempts <= 0)
        {
            return Result.Fail(DomainErrors.Otp.InvalidSettings);
        }

        return Result.Ok(new OtpChallenge(mobile, purpose, codeHash, utcNow.Add(lifetime), maxAttempts));
    }

    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAt;

    /// <summary>True while the code can still be verified.</summary>
    public bool IsOpen(DateTime utcNow) =>
        ConsumedAt is null && VerifiedAt is null && !IsExpired(utcNow) && Attempts < MaxAttempts;

    /// <summary>Checks a submitted code (as hash). Every call on an open challenge counts as one attempt.</summary>
    public Result Verify(string submittedCodeHash, DateTime utcNow)
    {
        if (ConsumedAt is not null || VerifiedAt is not null)
        {
            return Result.Fail(DomainErrors.Otp.AlreadyUsed);
        }

        if (IsExpired(utcNow))
        {
            return Result.Fail(DomainErrors.Otp.Expired);
        }

        if (Attempts >= MaxAttempts)
        {
            return Result.Fail(DomainErrors.Otp.TooManyAttempts);
        }

        Attempts++;

        if (!HashesEqual(CodeHash, submittedCodeHash))
        {
            return Attempts >= MaxAttempts
                ? Result.Fail(DomainErrors.Otp.TooManyAttempts)
                : Result.Fail(DomainErrors.Otp.Invalid);
        }

        VerifiedAt = utcNow;
        return Result.Ok();
    }

    /// <summary>Marks a verified code as used, after the action it protects succeeded.</summary>
    public Result Consume(DateTime utcNow)
    {
        if (ConsumedAt is not null)
        {
            return Result.Fail(DomainErrors.Otp.AlreadyUsed);
        }

        if (VerifiedAt is null)
        {
            return Result.Fail(DomainErrors.Otp.NotVerified);
        }

        ConsumedAt = utcNow;
        return Result.Ok();
    }

    private static bool HashesEqual(string expected, string? actual) =>
        actual is not null
        && CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(expected), Encoding.UTF8.GetBytes(actual));
}
