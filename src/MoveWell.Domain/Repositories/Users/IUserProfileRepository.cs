using CookieBytes.Core.Domain.Interfaces;
using MoveWell.Domain.Aggregates.UserProfiles;

namespace MoveWell.Domain.Repositories.Users;

public interface IUserProfileRepository : IRepository<UserProfile>
{
    Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
