using CookieBytes.Core.Application.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoveWell.Common.Identity;
using MoveWell.Domain.Aggregates.UserProfiles;
using MoveWell.Domain.Shared.Enumerations;
using MoveWell.Domain.Shared.ValueObjects;
using MoveWell.Infrastructure.Identity;
using MoveWell.Infrastructure.Options;
using MoveWell.Infrastructure.Persistence;

namespace MoveWell.Infrastructure.Seeding;

/// <summary>Idempotent: ensures the three roles exist and, when configured, the first Admin (with profile).</summary>
public sealed class IdentitySeeder
{
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly MoveWellDbContext _context;
    private readonly IDateTimeProvider _clock;
    private readonly AdminSeedOptions _adminSeed;
    private readonly ILogger<IdentitySeeder> _logger;

    public IdentitySeeder(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        MoveWellDbContext context,
        IDateTimeProvider clock,
        IOptions<AdminSeedOptions> adminSeed,
        ILogger<IdentitySeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
        _clock = clock;
        _adminSeed = adminSeed.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        foreach (var role in Roles.All)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                EnsureSucceeded(await _roleManager.CreateAsync(new ApplicationRole(role)), $"create role {role}");
            }
        }

        await SeedFirstAdminAsync(cancellationToken);
    }

    private async Task SeedFirstAdminAsync(CancellationToken cancellationToken)
    {
        if ((await _userManager.GetUsersInRoleAsync(Roles.Admin)).Count > 0)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_adminSeed.Mobile) || string.IsNullOrWhiteSpace(_adminSeed.Password))
        {
            _logger.LogWarning("No Admin exists and Admin:Seed:Mobile/Password are not configured; skipping first-admin seed.");
            return;
        }

        var mobile = MobileNumber.Create(_adminSeed.Mobile);
        if (mobile.IsFailed)
        {
            throw new InvalidOperationException("Admin:Seed:Mobile is not a valid Iranian mobile number.");
        }

        var user = await _userManager.FindByNameAsync(mobile.Value.Value);
        if (user is null)
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = mobile.Value.Value,
                PhoneNumber = mobile.Value.Value,
                PhoneNumberConfirmed = true,
            };
            EnsureSucceeded(await _userManager.CreateAsync(user, _adminSeed.Password), "create first admin");
        }

        EnsureSucceeded(await _userManager.AddToRoleAsync(user, Roles.Admin), "add first admin to role");

        if (!_context.UserProfiles.Any(profile => profile.UserId == user.Id))
        {
            var profile = UserProfile.Create(user.Id, _adminSeed.DisplayName, Culture.Fa, _clock.UtcNow);
            if (profile.IsFailed)
            {
                throw new InvalidOperationException("Admin:Seed:DisplayName is invalid: " + profile.Errors[0].Message);
            }

            _context.UserProfiles.Add(profile.Value);
            await _context.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation("First Admin seeded for {Mobile}.", mobile.Value.Masked);
    }

    private static void EnsureSucceeded(IdentityResult result, string action)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Identity seeding failed to {action}: " + string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }
}
