using System.Collections;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace AxisCore.Mapper;

/// <summary>
/// High-performance object mapper with automatic type conversion.
/// </summary>
public sealed class Mapper : IMapper
{
    private readonly MapperConfiguration _configuration;
    private static readonly ConcurrentDictionary<(Type Source, Type Destination), object> _mappingCache = new();

    /// <summary>
    /// Initializes a new instance of the Mapper class.
    /// </summary>
    /// <param name="configuration">Optional configuration</param>
    public Mapper(MapperConfiguration? configuration = null)
    {
        _configuration = configuration ?? new MapperConfiguration();
    }

    /// <inheritdoc />
    public TDestination Map<TDestination>(object source)
    {
        if (source == null)
        {
            if (_configuration.IgnoreNullSources)
            {
                return default!;
            }
            throw new ArgumentNullException(nameof(source));
        }

        return (TDestination)MapInternal(source, typeof(TDestination));
    }

    /// <inheritdoc />
    public TDestination Map<TSource, TDestination>(TSource source)
    {
        if (source == null)
        {
            if (_configuration.IgnoreNullSources)
            {
                return default!;
            }
            throw new ArgumentNullException(nameof(source));
        }

        // Check for custom mapping
        if (_configuration.TryGetCustomMapping<TSource, TDestination>(out var customMapping))
        {
            return customMapping!(source);
        }

        return (TDestination)MapInternal(source, typeof(TDestination));
    }

    /// <inheritdoc />
    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (destination == null)
        {
            throw new ArgumentNullException(nameof(destination));
        }

        MapToExisting(source, destination);
        return destination;
    }

    private object MapInternal(object source, Type destinationType)
    {
        var sourceType = source.GetType();

        // Handle same type
        if (sourceType == destinationType)
        {
            return source;
        }

        // Handle nullable types
        var underlyingDestType = Nullable.GetUnderlyingType(destinationType) ?? destinationType;
        var underlyingSourceType = Nullable.GetUnderlyingType(sourceType) ?? sourceType;

        // Handle type conversion for primitives and simple types
        if (CanDirectConvert(underlyingSourceType, underlyingDestType))
        {
            return ConvertValue(source, underlyingDestType);
        }

        // Handle collections
        if (IsCollection(destinationType))
        {
            return MapCollection(source, sourceType, destinationType);
        }

        // Handle dictionaries
        if (IsDictionary(destinationType))
        {
            return MapDictionary(source, sourceType, destinationType);
        }

        // Handle complex objects
        return MapComplexObject(source, sourceType, destinationType);
    }

    private void MapToExisting(object source, object destination)
    {
        var sourceType = source.GetType();
        var destinationType = destination.GetType();

        var sourceProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

        var destProperties = destinationType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        foreach (var destProp in destProperties)
        {
            if (sourceProperties.TryGetValue(destProp.Name, out var sourceProp))
            {
                try
                {
                    var sourceValue = sourceProp.GetValue(source);
                    if (sourceValue != null)
                    {
                        var convertedValue = MapInternal(sourceValue, destProp.PropertyType);
                        destProp.SetValue(destination, convertedValue);
                    }
                    else if (IsNullable(destProp.PropertyType))
                    {
                        destProp.SetValue(destination, null);
                    }
                }
                catch (Exception ex) when (_configuration.ThrowOnMappingFailure)
                {
                    throw new InvalidOperationException(
                        $"Failed to map property '{destProp.Name}' from {sourceType.Name} to {destinationType.Name}", ex);
                }
            }
        }
    }

    private object MapComplexObject(object source, Type sourceType, Type destinationType)
    {
        // Try to get from cache
        var cacheKey = (sourceType, destinationType);
        if (_mappingCache.TryGetValue(cacheKey, out var cachedMapper))
        {
            var mapperFunc = (Func<object, object>)cachedMapper;
            return mapperFunc(source);
        }

        // Create instance
        var destination = Activator.CreateInstance(destinationType)
            ?? throw new InvalidOperationException($"Cannot create instance of {destinationType.Name}");

        MapToExisting(source, destination);

        return destination;
    }

    private object MapCollection(object source, Type sourceType, Type destinationType)
    {
        if (source is not IEnumerable sourceEnumerable)
        {
            throw new InvalidOperationException($"Source type {sourceType.Name} is not enumerable");
        }

        var destElementType = GetCollectionElementType(destinationType);
        if (destElementType == null)
        {
            throw new InvalidOperationException($"Cannot determine element type for {destinationType.Name}");
        }

        var items = new List<object>();
        foreach (var item in sourceEnumerable)
        {
            if (item != null)
            {
                items.Add(MapInternal(item, destElementType));
            }
        }

        // Handle arrays
        if (destinationType.IsArray)
        {
            var array = Array.CreateInstance(destElementType, items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                array.SetValue(items[i], i);
            }
            return array;
        }

        // Handle List<T>
        if (destinationType.IsGenericType && destinationType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var listType = typeof(List<>).MakeGenericType(destElementType);
            var list = (IList)Activator.CreateInstance(listType)!;
            foreach (var item in items)
            {
                list.Add(item);
            }
            return list;
        }

        // Handle IEnumerable<T>, ICollection<T>
        if (destinationType.IsInterface || destinationType.IsAbstract)
        {
            var listType = typeof(List<>).MakeGenericType(destElementType);
            var list = (IList)Activator.CreateInstance(listType)!;
            foreach (var item in items)
            {
                list.Add(item);
            }
            return list;
        }

        throw new InvalidOperationException($"Unsupported collection type: {destinationType.Name}");
    }

    private object MapDictionary(object source, Type sourceType, Type destinationType)
    {
        if (source is not IDictionary sourceDictionary)
        {
            throw new InvalidOperationException($"Source type {sourceType.Name} is not a dictionary");
        }

        var destKeyType = destinationType.GetGenericArguments()[0];
        var destValueType = destinationType.GetGenericArguments()[1];

        var dictType = typeof(Dictionary<,>).MakeGenericType(destKeyType, destValueType);
        var dictionary = (IDictionary)Activator.CreateInstance(dictType)!;

        foreach (DictionaryEntry entry in sourceDictionary)
        {
            var key = entry.Key != null ? ConvertValue(entry.Key, destKeyType) : null;
            var value = entry.Value != null ? MapInternal(entry.Value, destValueType) : null;

            if (key != null)
            {
                dictionary.Add(key, value);
            }
        }

        return dictionary;
    }

    private static object ConvertValue(object value, Type targetType)
    {
        var sourceType = value.GetType();

        // Handle same type
        if (sourceType == targetType)
        {
            return value;
        }

        // Handle nullable
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType != null)
        {
            if (value == null)
            {
                return null!;
            }
            targetType = underlyingType;
        }

        // Handle string conversions
        if (targetType == typeof(string))
        {
            return value.ToString() ?? string.Empty;
        }

        if (sourceType == typeof(string))
        {
            var stringValue = (string)value;

            // Handle numeric conversions from string
            if (targetType == typeof(int) || targetType == typeof(int?))
            {
                if (int.TryParse(stringValue, out var intResult))
                    return intResult;
                throw new InvalidOperationException($"Cannot convert '{stringValue}' to int");
            }

            if (targetType == typeof(long) || targetType == typeof(long?))
            {
                if (long.TryParse(stringValue, out var longResult))
                    return longResult;
                throw new InvalidOperationException($"Cannot convert '{stringValue}' to long");
            }

            if (targetType == typeof(double) || targetType == typeof(double?))
            {
                if (double.TryParse(stringValue, out var doubleResult))
                    return doubleResult;
                throw new InvalidOperationException($"Cannot convert '{stringValue}' to double");
            }

            if (targetType == typeof(decimal) || targetType == typeof(decimal?))
            {
                if (decimal.TryParse(stringValue, out var decimalResult))
                    return decimalResult;
                throw new InvalidOperationException($"Cannot convert '{stringValue}' to decimal");
            }

            if (targetType == typeof(bool) || targetType == typeof(bool?))
            {
                if (bool.TryParse(stringValue, out var boolResult))
                    return boolResult;
                throw new InvalidOperationException($"Cannot convert '{stringValue}' to bool");
            }

            if (targetType == typeof(Guid) || targetType == typeof(Guid?))
            {
                if (Guid.TryParse(stringValue, out var guidResult))
                    return guidResult;
                throw new InvalidOperationException($"Cannot convert '{stringValue}' to Guid");
            }

            if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
            {
                if (DateTime.TryParse(stringValue, out var dateResult))
                    return dateResult;
                throw new InvalidOperationException($"Cannot convert '{stringValue}' to DateTime");
            }
        }

        // Try Convert.ChangeType for compatible types
        try
        {
            return Convert.ChangeType(value, targetType);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Cannot convert value of type {sourceType.Name} to {targetType.Name}", ex);
        }
    }

    private static bool CanDirectConvert(Type sourceType, Type destType)
    {
        if (sourceType == destType)
            return true;

        // Check if both are primitives or common value types
        if (IsPrimitiveOrCommon(sourceType) && IsPrimitiveOrCommon(destType))
            return true;

        // Check if conversion is possible via Convert.ChangeType
        if (typeof(IConvertible).IsAssignableFrom(sourceType) &&
            typeof(IConvertible).IsAssignableFrom(destType))
            return true;

        return false;
    }

    private static bool IsPrimitiveOrCommon(Type type)
    {
        return type.IsPrimitive ||
               type == typeof(string) ||
               type == typeof(decimal) ||
               type == typeof(DateTime) ||
               type == typeof(DateTimeOffset) ||
               type == typeof(TimeSpan) ||
               type == typeof(Guid);
    }

    private static bool IsCollection(Type type)
    {
        if (type == typeof(string))
            return false;

        return type.IsArray ||
               (type.IsGenericType && (
                   type.GetGenericTypeDefinition() == typeof(List<>) ||
                   type.GetGenericTypeDefinition() == typeof(IList<>) ||
                   type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
                   type.GetGenericTypeDefinition() == typeof(IEnumerable<>))) ||
               typeof(IEnumerable).IsAssignableFrom(type);
    }

    private static bool IsDictionary(Type type)
    {
        return type.IsGenericType &&
               (type.GetGenericTypeDefinition() == typeof(Dictionary<,>) ||
                type.GetGenericTypeDefinition() == typeof(IDictionary<,>));
    }

    private static Type? GetCollectionElementType(Type type)
    {
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        if (type.IsGenericType)
        {
            return type.GetGenericArguments()[0];
        }

        var enumerable = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerable?.GetGenericArguments()[0];
    }

    private static bool IsNullable(Type type)
    {
        return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
    }
}
