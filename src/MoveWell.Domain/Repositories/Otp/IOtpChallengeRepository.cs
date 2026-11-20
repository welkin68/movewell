using CookieBytes.Core.Domain.Interfaces;
using MoveWell.Domain.Aggregates.OtpChallenges;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Domain.Repositories.Otp;

public interface IOtpChallengeRepository : IRepository<OtpChallenge>
{
    /// <summary>Newest challenge for this mobile and purpose that is not consumed (it may be expired).</summary>
    Task<OtpChallenge?> GetLatestUnconsumedAsync(MobileNumber mobile, OtpPurpose purpose, CancellationToken cancellationToken = default);

    /// <summary>How many codes were issued to this mobile since <paramref name="sinceUtc"/> (send limits).</summary>
    Task<int> CountIssuedSinceAsync(MobileNumber mobile, DateTime sinceUtc, CancellationToken cancellationToken = default);
}
