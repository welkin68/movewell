using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Application.Abstractions.Identity;

/// <summary>What the application needs to know about an Identity account (no Identity types leak into Application).</summary>
public sealed record AuthUser(Guid UserId, MobileNumber Mobile, IReadOnlyList<string> Roles);
