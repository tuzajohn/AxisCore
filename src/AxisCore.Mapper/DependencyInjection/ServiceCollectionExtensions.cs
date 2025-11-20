using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AxisCore.Mapper.DependencyInjection;

/// <summary>
/// Extension methods for configuring AxisCore.Mapper services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers AxisCore.Mapper services with the service collection.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Optional configuration action</param>
    /// <param name="lifetime">Service lifetime for the mapper (default: Singleton)</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMapper(
        this IServiceCollection services,
        Action<MapperConfiguration>? configure = null,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var configuration = new MapperConfiguration();
        configure?.Invoke(configuration);

        services.TryAdd(new ServiceDescriptor(typeof(MapperConfiguration), configuration));
        services.TryAdd(new ServiceDescriptor(typeof(IMapper), typeof(Mapper), lifetime));

        return services;
    }

    /// <summary>
    /// Registers AxisCore.Mapper services with a pre-configured MapperConfiguration.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Pre-configured mapper configuration</param>
    /// <param name="lifetime">Service lifetime for the mapper (default: Singleton)</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddMapper(
        this IServiceCollection services,
        MapperConfiguration configuration,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        services.TryAddSingleton(configuration);
        services.TryAdd(new ServiceDescriptor(typeof(IMapper), typeof(Mapper), lifetime));

        return services;
    }
}
