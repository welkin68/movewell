using System.Security.Cryptography;
using CookieBytes.Core.Application.Abstractions;
using CookieBytes.Core.Infrastructure.DependencyInjection;
using CookieBytes.Core.Results.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using MoveWell.Application.Abstractions.Identity;
using MoveWell.Application.Abstractions.Messaging;
using MoveWell.Application.Abstractions.Persistence;
using MoveWell.Application.Abstractions.Security;
using MoveWell.Application.DependencyInjection;
using MoveWell.Application.Options;
using MoveWell.Infrastructure.Identity;
using MoveWell.Infrastructure.Messaging.Sms;
using MoveWell.Infrastructure.Options;
using MoveWell.Infrastructure.Persistence;
using MoveWell.Infrastructure.Security;
using MoveWell.Infrastructure.Seeding;
using MoveWell.Infrastructure.Services;

namespace MoveWell.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public const string ConnectionStringName = "Default";
    public const string ReadyTag = "ready";
    public const string TestingEnvironment = "Testing";

    /// <summary>Everything the Api host needs below the HTTP layer: Core/MediatR pipeline, EF, Identity, auth services, health checks.</summary>
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
        services.AddMoveWellApplication();

        services.AddOptions<DatabaseOptions>().Bind(configuration.GetSection(DatabaseOptions.SectionName));
        services.AddOptions<AdminSeedOptions>().Bind(configuration.GetSection(AdminSeedOptions.SectionName));
        services.AddOptions<OtpOptions>()
            .Bind(configuration.GetSection(OtpOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var connectionString = configuration.GetConnectionString(ConnectionStringName)
            ?? throw new InvalidOperationException($"ConnectionStrings:{ConnectionStringName} is not configured.");

        // No retrying execution strategy: handlers open explicit transactions (registration), which it does not allow.
        services.AddDbContext<MoveWellDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IMoveWellUnitOfWork, MoveWellUnitOfWork>();

        services.AddMoveWellIdentity();
        services.AddScoped<IdentitySeeder>();
        services.AddMoveWellAuthServices(configuration, environment);

        services.AddHealthChecks()
            .AddDbContextCheck<MoveWellDbContext>("database", HealthStatus.Unhealthy, [ReadyTag]);

        return services;
    }

    public static bool IsDevelopmentOrTesting(this IHostEnvironment environment) =>
        environment.IsDevelopment() || environment.IsEnvironment(TestingEnvironment);

    /// <summary>Identity stores and rules (ADR-0003).</summary>
    private static void AddMoveWellIdentity(this IServiceCollection services)
    {
        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredUniqueChars = 1;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = true;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<MoveWellDbContext>();
    }

    /// <summary>Tokens, OTP hashing/generation and SMS. Secrets get a throw-away random value in Development/Testing only.</summary>
    private static void AddMoveWellAuthServices(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var devFallback = environment.IsDevelopmentOrTesting();
        var ephemeralJwtKey = RandomSecret();
        var ephemeralOtpSecret = RandomSecret();

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .PostConfigure(options =>
            {
                if (string.IsNullOrWhiteSpace(options.SigningKey) && devFallback)
                {
                    options.SigningKey = ephemeralJwtKey; // tokens die on restart; fine for dev
                }
            })
            .Validate(options => options.SigningKey.Length >= JwtOptions.MinimumKeyLength,
                $"Jwt:SigningKey must be at least {JwtOptions.MinimumKeyLength} characters (set it with user-secrets or an environment variable).")
            .Validate(options => options.AccessTokenMinutes is > 0 and <= 60, "Jwt:AccessTokenMinutes must be 1-60.")
            .Validate(options => options.RefreshTokenDays is > 0 and <= 90, "Jwt:RefreshTokenDays must be 1-90.")
            .ValidateOnStart();

        services.AddOptions<OtpSecurityOptions>()
            .Bind(configuration.GetSection(OtpSecurityOptions.SectionName))
            .PostConfigure(options =>
            {
                if (string.IsNullOrWhiteSpace(options.HashSecret) && devFallback)
                {
                    options.HashSecret = ephemeralOtpSecret;
                }
            })
            .Validate(options => options.HashSecret.Length >= OtpSecurityOptions.MinimumSecretLength,
                $"Otp:HashSecret must be at least {OtpSecurityOptions.MinimumSecretLength} characters (user-secrets or environment variable).")
            .ValidateOnStart();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddSingleton<IOtpCodeGenerator, OtpCodeGenerator>();
        services.AddSingleton<IOtpHasher, OtpHasher>();

        var sms = configuration.GetSection(SmsOptions.SectionName).Get<SmsOptions>() ?? new SmsOptions();
        switch (sms.Provider)
        {
            case SmsOptions.DevProvider when devFallback:
                services.AddSingleton<ISmsSender, DevSmsSender>();
                break;
            case SmsOptions.DevProvider:
                throw new InvalidOperationException("Sms:Provider 'Dev' writes codes to the log and is allowed only in Development/Testing.");
            case SmsOptions.NullProvider:
                services.AddSingleton<ISmsSender, NullSmsSender>();
                break;
            default:
                throw new InvalidOperationException($"Unknown Sms:Provider '{sms.Provider}'.");
        }
    }

    private static string RandomSecret() => WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(48));
}
