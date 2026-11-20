using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MoveWell.Common.Identity;
using MoveWell.Infrastructure.Identity;
using MoveWell.Infrastructure.Options;

namespace MoveWell.Infrastructure.DependencyInjection;

public static class JwtAuthenticationServiceCollectionExtensions
{
    /// <summary>Validates the Api's own JWTs and registers the shared policies (AdminOnly, ContentEditor, Member).</summary>
    public static IServiceCollection AddMoveWellJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        // Configured from validated JwtOptions (so the dev fallback key is the same one TokenService signs with).
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
            {
                var jwt = jwtOptions.Value;
                bearer.MapInboundClaims = false; // keep "sub", "role", "name" as issued
                bearer.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = TokenService.CreateSigningKey(jwt.SigningKey),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "name",
                    RoleClaimType = "role",
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(Roles.Admin))
            .AddPolicy(Policies.ContentEditor, policy => policy.RequireRole(Roles.Admin, Roles.Content))
            .AddPolicy(Policies.Member, policy => policy.RequireAuthenticatedUser());

        return services;
    }
}
