using System.Security.Claims;
using CookieBytes.Core.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace MoveWell.Infrastructure.Identity;

/// <summary>Current user id for audit columns: the JWT <c>sub</c> claim, or "System" outside a signed-in request.</summary>
public sealed class HttpCurrentUserService : ICurrentUserService
{
    public const string SystemUser = "System";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var id = user?.FindFirstValue("sub") ?? user?.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrWhiteSpace(id) ? SystemUser : id;
        }
    }
}
