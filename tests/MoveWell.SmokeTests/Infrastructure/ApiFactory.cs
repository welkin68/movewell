using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MoveWell.Application.Abstractions.Messaging;
using MoveWell.Infrastructure.Persistence;

namespace MoveWell.SmokeTests.Infrastructure;

/// <summary>
/// The real Api in memory, environment "Testing", against a fresh LocalDB database that is dropped afterwards.
/// SMS is captured instead of sent; rate limits are raised so tests do not trip them (OTP per-mobile limits still apply).
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<MoveWell.Api.Localization.ErrorMessages>, IAsyncLifetime
{
    private readonly string _databaseName = $"MoveWell_Smoke_{Guid.NewGuid():N}";

    /// <summary>Test-only staff account seeded into the throw-away database.</summary>
    public const string AdminMobile = "09129999999";
    public const string AdminPassword = "SmokeAdmin123";

    public CapturingSmsSender Sms { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default",
            $"Server=(localdb)\\MSSQLLocalDB;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True");
        builder.UseSetting("Database:MigrateOnStartup", "true");
        builder.UseSetting("Sms:Provider", "Null");
        builder.UseSetting("Admin:Seed:Mobile", AdminMobile);
        builder.UseSetting("Admin:Seed:Password", AdminPassword);
        builder.UseSetting("Admin:Seed:DisplayName", "Smoke Admin");
        builder.UseSetting("RateLimits:AuthPerMinute", "1000");
        builder.UseSetting("RateLimits:OtpSendPerHour", "1000");
        builder.UseSetting("Logging:Directory", Path.Combine(Path.GetTempPath(), "movewell-smoke-logs"));

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ISmsSender>();
            services.AddSingleton<ISmsSender>(Sms);
        });
    }

    public Task InitializeAsync()
    {
        _ = Server; // starts the host → migrations + seeding run
        return Task.CompletedTask;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await using (var scope = Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<MoveWellDbContext>().Database.EnsureDeletedAsync();
        }

        await base.DisposeAsync();
    }
}
