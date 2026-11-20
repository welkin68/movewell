namespace MoveWell.Common.Contracts.Auth;

public sealed record AuthSessionResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    AuthUserResponse User);

public sealed record AuthUserResponse(
    Guid Id,
    string Mobile,
    string DisplayName,
    string Culture,
    IReadOnlyList<string> Roles);
