namespace AxisCore.Mapper;

/// <summary>
/// Configuration for the mapper.
/// </summary>
public sealed class MapperConfiguration
{
    private readonly Dictionary<(Type Source, Type Destination), object> _customMappings = new();

    /// <summary>
    /// Gets whether to throw exceptions on mapping failures. Default is true.
    /// </summary>
    public bool ThrowOnMappingFailure { get; set; } = true;

    /// <summary>
    /// Gets whether to ignore null source values. Default is false.
    /// </summary>
    public bool IgnoreNullSources { get; set; } = false;

    /// <summary>
    /// Creates a custom mapping configuration.
    /// </summary>
    /// <typeparam name="TSource">Source type</typeparam>
    /// <typeparam name="TDestination">Destination type</typeparam>
    /// <param name="mappingFunc">Custom mapping function</param>
    public void CreateMap<TSource, TDestination>(Func<TSource, TDestination> mappingFunc)
    {
        if (mappingFunc == null)
        {
            throw new ArgumentNullException(nameof(mappingFunc));
        }

        _customMappings[(typeof(TSource), typeof(TDestination))] = mappingFunc;
    }

    /// <summary>
    /// Gets a custom mapping function if one exists.
    /// </summary>
    internal bool TryGetCustomMapping<TSource, TDestination>(out Func<TSource, TDestination>? mappingFunc)
    {
        if (_customMappings.TryGetValue((typeof(TSource), typeof(TDestination)), out var func))
        {
            mappingFunc = (Func<TSource, TDestination>)func;
            return true;
        }

        mappingFunc = null;
        return false;
    }
}
