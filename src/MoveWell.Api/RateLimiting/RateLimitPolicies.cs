using System.Threading.RateLimiting;
using MoveWell.Api.Http;
using MoveWell.Common.Contracts.Common;

namespace MoveWell.Api.RateLimiting;

/// <summary>
/// Per-IP limits (backend-guide §5.4). Per-mobile OTP limits live in the domain flow (OtpService);
/// these stop one machine from hammering many numbers or passwords.
/// </summary>
public static class RateLimitPolicies
{
    public const string Auth = "auth";
    public const string OtpSend = "otp-send";
    public const string SectionName = "RateLimits";

    public static IServiceCollection AddMoveWellRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var limits = configuration.GetSection(SectionName).Get<RateLimitOptions>() ?? new RateLimitOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(Auth, context => RateLimitPartition.GetFixedWindowLimiter(
                context.GetClientIp() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = limits.AuthPerMinute, Window = TimeSpan.FromMinutes(1) }));

            options.AddPolicy(OtpSend, context => RateLimitPartition.GetFixedWindowLimiter(
                context.GetClientIp() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = limits.OtpSendPerHour, Window = TimeSpan.FromHours(1) }));

            options.OnRejected = async (rejected, cancellationToken) =>
            {
                var http = rejected.HttpContext;
                var problem = ResultHttpExtensions.Problem(
                    [new Domain.Shared.Errors.DomainError(ApiErrorCodes.RateLimited, "Too many requests.", Domain.Shared.Errors.ErrorKind.TooManyRequests)],
                    http);
                await problem.ExecuteAsync(http);
            };
        });

        return services;
    }

    public sealed class RateLimitOptions
    {
        public int AuthPerMinute { get; set; } = 10;

        public int OtpSendPerHour { get; set; } = 20;
    }
}
