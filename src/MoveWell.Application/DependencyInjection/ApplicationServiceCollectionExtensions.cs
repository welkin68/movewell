using Microsoft.Extensions.DependencyInjection;
using MoveWell.Application.Services.Otp;

namespace MoveWell.Application.DependencyInjection;

/// <summary>
/// Application services that are not MediatR handlers (handlers/validators are scanned by CookieBytes.Core).
/// Options binding (e.g. <c>OtpOptions</c>) is done by the host in Infrastructure, which owns configuration packages.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddMoveWellApplication(this IServiceCollection services)
    {
        services.AddScoped<OtpService>();
        return services;
    }
}
