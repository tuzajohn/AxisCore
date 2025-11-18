using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AxisCore.Mediator.DependencyInjection;

/// <summary>
/// Extension methods for configuring AxisCore.Mediator services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers AxisCore.Mediator services with the service collection.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Optional configuration action</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMediator(
        this IServiceCollection services,
        Action<MediatorOptions>? configure = null)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var options = new MediatorOptions();
        configure?.Invoke(options);

        services.TryAddSingleton(options);
        services.TryAddTransient<IMediator, Mediator>();

        return services;
    }

    /// <summary>
    /// Registers AxisCore.Mediator services and scans assemblies for handlers.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assemblies">Assemblies to scan for handlers</param>
    /// <param name="configure">Optional configuration action</param>
    /// <param name="lifetime">Service lifetime for handlers (default: Transient)</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMediator(
        this IServiceCollection services,
        IEnumerable<Assembly> assemblies,
        Action<MediatorOptions>? configure = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (assemblies == null)
        {
            throw new ArgumentNullException(nameof(assemblies));
        }

        services.AddMediator(configure);

        foreach (var assembly in assemblies)
        {
            services.RegisterHandlersFromAssembly(assembly, lifetime);
        }

        return services;
    }

    /// <summary>
    /// Registers AxisCore.Mediator services and scans the calling assembly for handlers.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Optional configuration action</param>
    /// <param name="lifetime">Service lifetime for handlers (default: Transient)</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMediatorFromAssembly(
        this IServiceCollection services,
        Action<MediatorOptions>? configure = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.AddMediator(new[] { Assembly.GetCallingAssembly() }, configure, lifetime);
    }

    /// <summary>
    /// Registers AxisCore.Mediator services and scans the assembly containing the specified type for handlers.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="type">Type in the assembly to scan</param>
    /// <param name="configure">Optional configuration action</param>
    /// <param name="lifetime">Service lifetime for handlers (default: Transient)</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMediatorFromAssemblyContaining(
        this IServiceCollection services,
        Type type,
        Action<MediatorOptions>? configure = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        return services.AddMediator(new[] { type.Assembly }, configure, lifetime);
    }

    /// <summary>
    /// Registers AxisCore.Mediator services and scans the assembly containing the specified type for handlers.
    /// </summary>
    /// <typeparam name="T">Type in the assembly to scan</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Optional configuration action</param>
    /// <param name="lifetime">Service lifetime for handlers (default: Transient)</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMediatorFromAssemblyContaining<T>(
        this IServiceCollection services,
        Action<MediatorOptions>? configure = null,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        return services.AddMediatorFromAssemblyContaining(typeof(T), configure, lifetime);
    }

    /// <summary>
    /// Registers handlers from the specified assembly.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="assembly">Assembly to scan</param>
    /// <param name="lifetime">Service lifetime for handlers</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection RegisterHandlersFromAssembly(
        this IServiceCollection services,
        Assembly assembly,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (assembly == null)
        {
            throw new ArgumentNullException(nameof(assembly));
        }

        var types = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
            .ToList();

        foreach (var type in types)
        {
            // Register request handlers
            RegisterInterfaceImplementations(services, type, typeof(IRequestHandler<,>), lifetime);

            // Register notification handlers
            RegisterInterfaceImplementations(services, type, typeof(INotificationHandler<>), lifetime);

            // Register stream handlers
            RegisterInterfaceImplementations(services, type, typeof(IStreamRequestHandler<,>), lifetime);

            // Register pipeline behaviors
            RegisterInterfaceImplementations(services, type, typeof(IPipelineBehavior<,>), lifetime);

            // Register pre-processors
            RegisterInterfaceImplementations(services, type, typeof(IRequestPreProcessor<>), lifetime);

            // Register post-processors
            RegisterInterfaceImplementations(services, type, typeof(IRequestPostProcessor<,>), lifetime);
        }

        return services;
    }

    /// <summary>
    /// Manually registers a request handler.
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <typeparam name="THandler">Handler type</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="lifetime">Service lifetime</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddRequestHandler<TRequest, TResponse, THandler>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TRequest : IRequest<TResponse>
        where THandler : class, IRequestHandler<TRequest, TResponse>
    {
        services.Add(new ServiceDescriptor(typeof(IRequestHandler<TRequest, TResponse>), typeof(THandler), lifetime));
        return services;
    }

    /// <summary>
    /// Manually registers a notification handler.
    /// </summary>
    /// <typeparam name="TNotification">Notification type</typeparam>
    /// <typeparam name="THandler">Handler type</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="lifetime">Service lifetime</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddNotificationHandler<TNotification, THandler>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TNotification : INotification
        where THandler : class, INotificationHandler<TNotification>
    {
        services.Add(new ServiceDescriptor(typeof(INotificationHandler<TNotification>), typeof(THandler), lifetime));
        return services;
    }

    /// <summary>
    /// Manually registers a pipeline behavior.
    /// </summary>
    /// <typeparam name="TRequest">Request type</typeparam>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <typeparam name="TBehavior">Behavior type</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="lifetime">Service lifetime</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddPipelineBehavior<TRequest, TResponse, TBehavior>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TRequest : IRequest<TResponse>
        where TBehavior : class, IPipelineBehavior<TRequest, TResponse>
    {
        services.Add(new ServiceDescriptor(typeof(IPipelineBehavior<TRequest, TResponse>), typeof(TBehavior), lifetime));
        return services;
    }

    private static void RegisterInterfaceImplementations(
        IServiceCollection services,
        Type implementationType,
        Type interfaceType,
        ServiceLifetime lifetime)
    {
        var interfaces = implementationType.GetInterfaces()
            .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType)
            .ToList();

        foreach (var @interface in interfaces)
        {
            services.Add(new ServiceDescriptor(@interface, implementationType, lifetime));
        }
    }
}
