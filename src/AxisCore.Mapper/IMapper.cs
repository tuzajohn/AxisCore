namespace AxisCore.Mapper;

/// <summary>
/// Defines a mapper for converting objects from one type to another.
/// </summary>
public interface IMapper
{
    /// <summary>
    /// Maps an object to a new instance of the destination type.
    /// </summary>
    /// <typeparam name="TDestination">The type to map to</typeparam>
    /// <param name="source">The source object</param>
    /// <returns>A new instance of TDestination with mapped values</returns>
    TDestination Map<TDestination>(object source);

    /// <summary>
    /// Maps an object to a new instance of the destination type.
    /// </summary>
    /// <typeparam name="TSource">The type to map from</typeparam>
    /// <typeparam name="TDestination">The type to map to</typeparam>
    /// <param name="source">The source object</param>
    /// <returns>A new instance of TDestination with mapped values</returns>
    TDestination Map<TSource, TDestination>(TSource source);

    /// <summary>
    /// Maps an object to an existing destination instance.
    /// </summary>
    /// <typeparam name="TSource">The type to map from</typeparam>
    /// <typeparam name="TDestination">The type to map to</typeparam>
    /// <param name="source">The source object</param>
    /// <param name="destination">The destination object</param>
    /// <returns>The destination object with mapped values</returns>
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);
}
