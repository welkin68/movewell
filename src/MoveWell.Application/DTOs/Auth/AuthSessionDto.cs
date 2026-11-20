namespace MoveWell.Application.DTOs.Auth;

public sealed record AuthSessionDto(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    AuthUserDto User);

public sealed record AuthUserDto(
    Guid Id,
    string Mobile,
    string DisplayName,
    string Culture,
    IReadOnlyList<string> Roles);
