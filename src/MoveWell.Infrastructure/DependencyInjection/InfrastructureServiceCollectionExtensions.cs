using CookieBytes.Core.Application.Abstractions;
using CookieBytes.Core.Infrastructure.DependencyInjection;
using CookieBytes.Core.Results.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using MoveWell.Application.Abstractions.Persistence;
using MoveWell.Infrastructure.Identity;
using MoveWell.Infrastructure.Options;
using MoveWell.Infrastructure.Persistence;
using MoveWell.Infrastructure.Seeding;
using MoveWell.Infrastructure.Services;

namespace MoveWell.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public const string ConnectionStringName = "Default";
    public const string ReadyTag = "ready";

    /// <summary>Everything the Api host needs below the HTTP layer: Core/MediatR pipeline, EF, Identity, seeders, health checks.</summary>
    public static IServiceCollection AddMoveWellInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<IDateTimeProvider, UtcDateTimeProvider>();
        services.AddScoped<ICurrentUserService, HttpCurrentUserService>();

        services.AddCookieBytesCore(configuration, environment, options =>
        {
            options.ValidationMode = CookieBytesCoreValidationMode.FluentResult;
            options.RegisterDefaultAuditServices = false;
            options.MediatRAssemblies.Add(typeof(Application.AssemblyReference).Assembly);
        });
        services.AddCookieBytesCoreResults();

        services.AddOptions<DatabaseOptions>().Bind(configuration.GetSection(DatabaseOptions.SectionName));
        services.AddOptions<AdminSeedOptions>().Bind(configuration.GetSection(AdminSeedOptions.SectionName));

        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"ConnectionStrings:{ConnectionStringName} is not configured.");

        services.AddDbContext<MoveWellDbContext>(options => options.UseSqlServer(
            connectionString,
            sql => sql.EnableRetryOnFailure(maxRetryCount: 3)));
        services.AddScoped<IMoveWellUnitOfWork, MoveWellUnitOfWork>();

        services.AddMoveWellIdentity();
        services.AddScoped<IdentitySeeder>();

        services.AddHealthChecks()
            .AddDbContextCheck<MoveWellDbContext>("database", HealthStatus.Unhealthy, [ReadyTag]);

        return services;
    }

    /// <summary>Identity stores and rules (ADR-0003). Token issuing is added in the auth slice.</summary>
    private static void AddMoveWellIdentity(this IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<MoveWellDbContext>();
    }
}
