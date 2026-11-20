using CookieBytes.Core.Application.Abstractions;
using CookieBytes.Core.Infrastructure.Persistence;
using MoveWell.Application.Abstractions.Persistence;
using MoveWell.Domain.Aggregates.OtpChallenges;
using MoveWell.Domain.Aggregates.UserProfiles;
using MoveWell.Domain.Repositories.Otp;
using MoveWell.Domain.Repositories.Users;
using MoveWell.Infrastructure.Repositories.Otp;
using MoveWell.Infrastructure.Repositories.Users;

namespace MoveWell.Infrastructure.Persistence;

/// <summary>Repositories share one DbContext; SaveAndDispatchEventsAsync saves, then dispatches domain events (Core).</summary>
public sealed class MoveWellUnitOfWork : UnitOfWork<MoveWellDbContext>, IMoveWellUnitOfWork
{
    public MoveWellUnitOfWork(MoveWellDbContext context, IDomainEventDispatcher? domainEventDispatcher = null)
        : base(context, domainEventDispatcher)
    {
    }

    public IUserProfileRepository UserProfiles =>
        GetRepository<UserProfileRepository, UserProfile>(context => new UserProfileRepository(context));

    public IOtpChallengeRepository OtpChallenges =>
        GetRepository<OtpChallengeRepository, OtpChallenge>(context => new OtpChallengeRepository(context));
}
