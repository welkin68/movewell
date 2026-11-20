using System.Reflection;
using CookieBytes.Core.Application.Abstractions;
using CookieBytes.Core.Application.Abstractions.Modules;
using CookieBytes.Core.Application.Behaviors;
using CookieBytes.Core.Infrastructure.Events;
using CookieBytes.Core.Infrastructure.Modules;
using CookieBytes.Core.Infrastructure.Services;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
namespace CookieBytes.Core.Infrastructure.DependencyInjection;
/// <summary>
/// CookieBytes.Core dependency injection registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers CookieBytes.Core from <c>CookieBytes:Core</c> configuration and host environment.
    /// Sets <see cref="CookieBytesCoreOptions.RegisterDefaultAuditServices"/> in Development only.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="environment">Host environment.</param>
    /// <param name="configure">Optional additional configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCookieBytesCore(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        Action<CookieBytesCoreOptions>? configure = null)
    {
        services.TryAddSingleton<ICookieBytesConfiguration>(_ => new CookieBytesConfiguration(configuration));
        return services.AddCookieBytesCore(options =>
        {
            configuration.GetSection("CookieBytes:Core").Bind(options);
            if (environment.IsDevelopment())
            {
                options.RegisterDefaultAuditServices = true;
            }
            configure?.Invoke(options);
        });
    }
    /// <summary>
    /// Registers CookieBytes.Core application services, MediatR, validators, and optional event/outbox support.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration for CookieBytes.Core options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCookieBytesCore(
        this IServiceCollection services,
        Action<CookieBytesCoreOptions>? configure = null)
    {
        RegisterCookieBytesOptions(services, configure);
        var options = new CookieBytesCoreOptions();
        configure?.Invoke(options);
        if (!string.IsNullOrWhiteSpace(options.OutboxConnectionString))
        {
            options.OutboxDbContextRegistered = true;
        }
        RegisterFoundationServices(services, options);
        RegisterKernelServices(services);
        RegisterOutboxServices(services, options);
        RegisterCqrsPipeline(services, options);
        return services;
    }
    /// <summary>
    /// Registers <see cref="OutboxDbContext"/> for integration event outbox storage.
    /// Call when <see cref="CookieBytesCoreOptions.EnableOutbox"/> is true and no connection string was set on options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">DbContext options configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCookieBytesCoreOutbox(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configure)
    {
        services.AddOptions<CookieBytesCoreOptions>()
            .Configure(options => options.OutboxDbContextRegistered = true);
        services.AddDbContext<OutboxDbContext>(configure);
        return services;
    }
    /// <summary>
    /// Registers <typeparamref name="TProcessor"/> as <see cref="IOutboxProcessor"/>.
    /// Call when <see cref="CookieBytesCoreOptions.EnableOutbox"/> is true.
    /// </summary>
    /// <typeparam name="TProcessor">The outbox processor implementation.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCookieBytesCoreOutboxProcessor<TProcessor>(this IServiceCollection services)
        where TProcessor : class, IOutboxProcessor
    {
        services.AddOptions<CookieBytesCoreOptions>()
            .Configure(options => options.OutboxProcessorRegistered = true);
        services.TryAddScoped<IOutboxProcessor, TProcessor>();
        return services;
    }
    /// <summary>
    /// Registers <see cref="InProcessOutboxProcessor"/> for monolith/demo hosts that dispatch
    /// outbox messages to in-process <see cref="Application.Abstractions.Modules.IIntegrationEventHandler{TEvent}"/> handlers via <see cref="Application.Abstractions.Modules.IEventBus"/>.
    /// Requires a prior call to <see cref="AddCookieBytesCore"/> (kernel / event bus).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureEventTypes">Registers integration event CLR types for JSON deserialization.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCookieBytesCoreOutboxInProcessProcessor(
        this IServiceCollection services,
        Action<IntegrationEventTypeRegistry>? configureEventTypes = null)
    {
        if (ModuleServiceCollectionExtensions.GetKernelRegistrationState(services) is null)
        {
            throw new InvalidOperationException(
                "AddCookieBytesCoreOutboxInProcessProcessor requires AddCookieBytesCore to be called first.");
        }
        var registry = new IntegrationEventTypeRegistry();
        configureEventTypes?.Invoke(registry);
        services.TryAddSingleton(registry);
        return services.AddCookieBytesCoreOutboxProcessor<InProcessOutboxProcessor>();
    }
    private static void RegisterCookieBytesOptions(
        IServiceCollection services,
        Action<CookieBytesCoreOptions>? configure)
    {
        services.AddOptions<CookieBytesCoreOptions>()
            .Configure(configure ?? (_ => { }))
            .Configure(options =>
            {
                if (!string.IsNullOrWhiteSpace(options.OutboxConnectionString))
                {
                    options.OutboxDbContextRegistered = true;
                }
                if (options.RegisterNoOpOutboxProcessor)
                {
                    options.OutboxProcessorRegistered = true;
                }
            })
            .Validate(
                options => !options.EnableOutbox || options.OutboxDbContextRegistered,
                "EnableOutbox requires OutboxConnectionString on AddCookieBytesCore options or a call to AddCookieBytesCoreOutbox().")
            .Validate(
                options => !options.EnableOutbox || !options.ValidateOutboxProcessorOnStart || options.OutboxProcessorRegistered,
                "EnableOutbox requires IOutboxProcessor registration. Call AddCookieBytesCoreOutboxProcessor<T>() or set RegisterNoOpOutboxProcessor for local dev. See docs/OUTBOX.md.")
            .Validate(
                options => !options.EnableValidationPipeline
                    || options.ValidationMode != CookieBytesCoreValidationMode.FluentResult
                    || options.FluentResultsRegistered,
                "ValidationMode FluentResult requires AddCookieBytesCoreResults(). See docs/RESULTS.md.")
            .ValidateOnStart();
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<CookieBytesCoreOptions>, CookieBytesCoreAuditOptionsValidator>());
        services.TryAddEnumerable(
            ServiceDescriptor.Singleton<IValidateOptions<CookieBytesCoreOptions>, CookieBytesCoreOutboxOptionsValidator>());
    }
    private static void RegisterFoundationServices(IServiceCollection services, CookieBytesCoreOptions options)
    {
        if (options.RegisterDefaultAuditServices)
        {
            services.TryAddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
            services.TryAddScoped<ICurrentUserService, SystemCurrentUserService>();
        }
        if (options.EnableDomainEventDispatch)
        {
            services.TryAddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        }
    }
    private static void RegisterKernelServices(IServiceCollection services)
    {
        if (ModuleServiceCollectionExtensions.GetKernelRegistrationState(services) is not null)
        {
            return;
        }
        var kernelState = new KernelRegistrationState { KernelRegistered = true };
        services.AddSingleton(kernelState);
        services.TryAddSingleton<IModuleRegistry>(_ => kernelState.ModuleRegistry);
        services.TryAddSingleton<IPermissionRegistry>(_ => kernelState.PermissionRegistry);
        services.TryAddSingleton<IEventBus, InProcessEventBus>();
    }
    private static void RegisterOutboxServices(IServiceCollection services, CookieBytesCoreOptions options)
    {
        if (!options.EnableOutbox)
        {
            return;
        }
        services.TryAddScoped<IOutboxStore, OutboxStore>();
        services.TryAddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>();
        if (options.RegisterNoOpOutboxProcessor)
        {
            services.TryAddScoped<IOutboxProcessor, NoOpOutboxProcessor>();
        }
        if (!string.IsNullOrWhiteSpace(options.OutboxConnectionString))
        {
            services.AddDbContext<OutboxDbContext>(dbOptions =>
                dbOptions.UseSqlServer(options.OutboxConnectionString));
        }
    }
    private static void RegisterCqrsPipeline(IServiceCollection services, CookieBytesCoreOptions options)
    {
        var mediatRAssemblies = options.MediatRAssemblies.Count > 0
            ? options.MediatRAssemblies
            : [Assembly.GetCallingAssembly()];
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssemblies(mediatRAssemblies.ToArray());
            if (options.EnableValidationPipeline
                && options.ValidationMode == CookieBytesCoreValidationMode.Throw)
            {
                configuration.AddOpenBehavior(typeof(ValidationBehaviorThrows<,>));
            }
            if (options.EnableLoggingPipeline)
            {
                configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            }
        });
        services.AddValidatorsFromAssemblies(mediatRAssemblies, includeInternalTypes: true);
    }
}
