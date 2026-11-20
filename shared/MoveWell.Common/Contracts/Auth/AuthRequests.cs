namespace MoveWell.Common.Contracts.Auth;

// Request bodies of /api/v1/auth. Shared by Api, Web and (later) the mobile app.
// The UI language comes from the Accept-Language header, not from the body.

public sealed record StartRegistrationRequest(string Mobile);

public sealed record CompleteRegistrationRequest(string Mobile, string Code, string Password, string DisplayName);

public sealed record SignInRequest(string Mobile, string Password);

public sealed record RefreshSessionRequest(string RefreshToken);

public sealed record SignOutRequest(string RefreshToken);

public sealed record StartPasswordResetRequest(string Mobile);

public sealed record CompletePasswordResetRequest(string Mobile, string Code, string NewPassword);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
