using System.Text;
using FluentResults;
using Microsoft.AspNetCore.Identity;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Common.Errors;
using MoveWell.Common.Identity;
using MoveWell.Domain.Shared.ValueObjects;

namespace MoveWell.Infrastructure.Identity;

/// <summary>ASP.NET Core Identity behind the <see cref="IIdentityService"/> port. UserName = PhoneNumber = normalized mobile.</summary>
public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<bool> MobileExistsAsync(MobileNumber mobile, CancellationToken cancellationToken = default) =>
        await _userManager.FindByNameAsync(mobile.Value) is not null;

    public async Task<AuthUser?> FindByMobileAsync(MobileNumber mobile, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(mobile.Value);
        return user is null ? null : await ToAuthUserAsync(user);
    }

    public async Task<AuthUser?> FindByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : await ToAuthUserAsync(user);
    }

    public async Task<Result> ValidatePasswordAsync(string password, CancellationToken cancellationToken = default)
    {
        var probe = new ApplicationUser();
        foreach (var validator in _userManager.PasswordValidators)
        {
            var result = await validator.ValidateAsync(_userManager, probe, password);
            if (!result.Succeeded)
            {
                return ToFailure(result);
            }
        }

        return Result.Ok();
    }

    public async Task<Result<AuthUser>> CreateMemberAsync(MobileNumber mobile, string password, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = mobile.Value,
            PhoneNumber = mobile.Value,
            PhoneNumberConfirmed = true,
        };

        var created = await _userManager.CreateAsync(user, password);
        if (!created.Succeeded)
        {
            return created.Errors.Any(error => error.Code == nameof(IdentityErrorDescriber.DuplicateUserName))
                ? Result.Fail(AuthErrors.AlreadyRegistered)
                : ToFailure(created);
        }

        var role = await _userManager.AddToRoleAsync(user, Roles.Member);
        if (!role.Succeeded)
        {
            throw new InvalidOperationException("Could not add the Member role: " + Describe(role));
        }

        return Result.Ok(new AuthUser(user.Id, mobile, [Roles.Member]));
    }

    public async Task<Result<AuthUser>> CheckPasswordAsync(MobileNumber mobile, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(mobile.Value);
        if (user is null)
        {
            return Result.Fail(AuthErrors.InvalidCredentials);
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Result.Fail(AuthErrors.LockedOut);
        }

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            await _userManager.AccessFailedAsync(user);
            return await _userManager.IsLockedOutAsync(user)
                ? Result.Fail(AuthErrors.LockedOut)
                : Result.Fail(AuthErrors.InvalidCredentials);
        }

        if (!user.PhoneNumberConfirmed)
        {
            return Result.Fail(AuthErrors.MobileNotConfirmed);
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        return Result.Ok(await ToAuthUserAsync(user));
    }

    /// <summary>The caller already proved ownership with an OTP and validated the password policy.</summary>
    public async Task<Result> ResetPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Fail(AuthErrors.CodeNotFound);
        }

        user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, newPassword);
        user.LockoutEnd = null;
        user.AccessFailedCount = 0;
        await _userManager.UpdateSecurityStampAsync(user); // also saves the user

        return Result.Ok();
    }

    public async Task<Result> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Fail(AuthErrors.NotSignedIn);
        }

        var changed = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (changed.Succeeded)
        {
            return Result.Ok();
        }

        return changed.Errors.Any(error => error.Code == nameof(IdentityErrorDescriber.PasswordMismatch))
            ? Result.Fail(AuthErrors.CurrentPasswordWrong)
            : ToFailure(changed);
    }

    private async Task<AuthUser> ToAuthUserAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var mobile = MobileNumber.Create(user.UserName).Value;
        return new AuthUser(user.Id, mobile, roles.ToList());
    }

    /// <summary>Maps Identity errors to stable codes, e.g. <c>PasswordTooShort</c> → <c>password.too_short</c>.</summary>
    private static Result ToFailure(IdentityResult result) =>
        Result.Fail(result.Errors.Select(error => AuthErrors.WeakPassword(ToSnakeCode(error.Code))));

    private static string ToSnakeCode(string identityCode)
    {
        var trimmed = identityCode.StartsWith("Password", StringComparison.Ordinal) ? identityCode["Password".Length..] : identityCode;
        var builder = new StringBuilder();
        foreach (var ch in trimmed)
        {
            if (char.IsUpper(ch) && builder.Length > 0)
            {
                builder.Append('_');
            }

            builder.Append(char.ToLowerInvariant(ch));
        }

        return builder.Length == 0 ? "invalid" : builder.ToString();
    }

    private static string Describe(IdentityResult result) =>
        string.Join("; ", result.Errors.Select(error => error.Description));
}
