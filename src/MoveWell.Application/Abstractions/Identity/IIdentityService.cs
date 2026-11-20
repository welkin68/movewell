using FluentResults;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Application.Abstractions.Identity;

/// <summary>Port over ASP.NET Core Identity (accounts, passwords, roles, lockout). Implemented in Infrastructure.</summary>
public interface IIdentityService
{
    Task<bool> MobileExistsAsync(MobileNumber mobile, CancellationToken cancellationToken = default);

    Task<AuthUser?> FindByMobileAsync(MobileNumber mobile, CancellationToken cancellationToken = default);

    Task<AuthUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Checks the password against Identity's policy without creating anything.</summary>
    Task<Result> ValidatePasswordAsync(string password, CancellationToken cancellationToken = default);

    /// <summary>Creates a Member account with a confirmed mobile (the OTP was verified by the caller).</summary>
    Task<Result<AuthUser>> CreateMemberAsync(MobileNumber mobile, string password, CancellationToken cancellationToken = default);

    /// <summary>Password sign-in check including lockout and confirmed-mobile rules.</summary>
    Task<Result<AuthUser>> CheckPasswordAsync(MobileNumber mobile, string password, CancellationToken cancellationToken = default);

    Task<Result> ResetPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken = default);

    Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}
