using CookieBytes.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MoveWell.Domain.Aggregates.OtpChallenges;
using MoveWell.Domain.Aggregates.OtpChallenges.Enumerations;
using MoveWell.Domain.Repositories.Otp;
using MoveWell.Domain.Shared.ValueObjects;
using MoveWell.Infrastructure.Persistence;

namespace MoveWell.Infrastructure.Repositories.Otp;

public sealed class OtpChallengeRepository : Repository<OtpChallenge>, IOtpChallengeRepository
{
    public OtpChallengeRepository(MoveWellDbContext context)
        : base(context)
    {
    }

    public Task<OtpChallenge?> GetLatestUnconsumedAsync(
        MobileNumber mobile,
        OtpPurpose purpose,
        CancellationToken cancellationToken = default) =>
        DbSet
            .Where(otp => otp.Mobile == mobile && otp.Purpose == purpose && otp.ConsumedAt == null)
            .OrderByDescending(otp => otp.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<int> CountIssuedSinceAsync(
        MobileNumber mobile,
        DateTime sinceUtc,
        CancellationToken cancellationToken = default) =>
        DbSet.CountAsync(otp => otp.Mobile == mobile && otp.CreatedAt >= sinceUtc, cancellationToken);
}
