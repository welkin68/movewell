using CookieBytes.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using MoveWell.Domain.Aggregates.UserProfiles;
using MoveWell.Domain.Repositories.Users;
using MoveWell.Infrastructure.Persistence;

namespace MoveWell.Infrastructure.Repositories.Users;

public sealed class UserProfileRepository : Repository<UserProfile>, IUserProfileRepository
{
    public UserProfileRepository(MoveWellDbContext context)
        : base(context)
    {
    }

    public Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default) =>
        DbSet.FirstOrDefaultAsync(profile => profile.UserId == userId, cancellationToken);
}
