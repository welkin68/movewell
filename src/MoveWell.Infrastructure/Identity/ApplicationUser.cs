using Microsoft.AspNetCore.Identity;

namespace MoveWell.Infrastructure.Identity;

/// <summary>
/// Identity account: credentials, phone, lockout. UserName = PhoneNumber = normalized mobile (09xxxxxxxxx).
/// No person fields here; they live on the UserProfile aggregate (user-profile law).
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
}
