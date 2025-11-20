# AxisCore.Mapper

High-performance object mapper with automatic type conversion, collection handling, and compiled expressions for .NET 6+.

## Features

- **Fast**: Uses compiled expressions and caching for high performance
- **Automatic Type Conversion**: Converts between compatible types (int ↔ string, etc.)
- **Collection Support**: Maps List, Array, Dictionary, and IEnumerable
- **Nullable Handling**: Proper support for nullable types
- **Case-Insensitive Matching**: Property names matched case-insensitively
- **Custom Mappings**: Configure custom mapping logic
- **Zero Dependencies**: Only requires Microsoft.Extensions.DependencyInjection.Abstractions
- **Multi-Targeting**: Supports .NET 6, .NET 8, and .NET 9

## Installation

```bash
dotnet add package AxisCore.Mapper
```

## Quick Start

### Basic Usage

```csharp
using AxisCore.Mapper;

var mapper = new Mapper();

var source = new { Name = "John", Age = 30 };
var destination = mapper.Map<PersonDto>(source);

// Result: PersonDto { Name = "John", Age = "30" }
```

### Dependency Injection

```csharp
using AxisCore.Mapper.DependencyInjection;

services.AddMapper();

// Inject IMapper
public class MyService
{
    private readonly IMapper _mapper;

    public MyService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public PersonDto Convert(Person person)
    {
        return _mapper.Map<Person, PersonDto>(person);
    }
}
```

## Type Conversion

AxisCore.Mapper automatically converts between compatible types:

```csharp
// Int to String
var source = new { Age = 25 };
var result = mapper.Map<DestWithString>(source);
// result.Age = "25"

// String to Int (throws if conversion fails)
var source = new { Age = "30" };
var result = mapper.Map<DestWithInt>(source);
// result.Age = 30

// Invalid conversion throws exception
var source = new { Age = "hello" };
var result = mapper.Map<DestWithInt>(source);
// Throws: InvalidOperationException: Cannot convert 'hello' to int
```

### Supported Conversions

- Primitives: int, long, double, decimal, bool, etc.
- String ↔ Any primitive type
- Nullable types
- DateTime, DateTimeOffset, TimeSpan, Guid
- Any type implementing IConvertible

## Collection Mapping

```csharp
// List mapping with type conversion
var source = new { Numbers = new List<int> { 1, 2, 3 } };
var result = mapper.Map<DestWithStringList>(source);
// result.Numbers = ["1", "2", "3"]

// Array mapping
var source = new { Items = new[] { 1, 2, 3 } };
var result = mapper.Map<DestWithArray>(source);

// Dictionary mapping
var source = new { Data = new Dictionary<string, int> { ["one"] = 1 } };
var result = mapper.Map<DestWithDictionary>(source);
```

## Mapping to Existing Objects

```csharp
var source = new { Name = "Jane", Age = 28 };
var destination = new Person { Name = "Old Name", Age = 0 };

mapper.Map(source, destination);
// destination.Name = "Jane", destination.Age = 28
```

## Custom Mappings

```csharp
var config = new MapperConfiguration();
config.CreateMap<Person, PersonDto>(person => new PersonDto
{
    FullName = $"{person.FirstName} {person.LastName}",
    Age = person.Age.ToString(),
    Email = person.Email?.ToLower()
});

var mapper = new Mapper(config);
```

## Configuration Options

```csharp
services.AddMapper(config =>
{
    // Don't throw exceptions on mapping failures (default: true)
    config.ThrowOnMappingFailure = false;

    // Ignore null source values (default: false)
    config.IgnoreNullSources = false;

    // Add custom mappings
    config.CreateMap<Source, Dest>(s => new Dest { ... });
});
```

## Performance

AxisCore.Mapper is designed for high performance:

- **Compiled Expressions**: Mapping logic is compiled for fast execution
- **Caching**: Handler mappings are cached to avoid reflection overhead
- **ValueTask**: Zero-allocation for synchronous operations (when applicable)
- **Minimal Allocations**: Efficient object creation and property copying

## Examples

### Complex Object Mapping

```csharp
public class Order
{
    public int OrderId { get; set; }
    public List<OrderItem> Items { get; set; }
}

public class OrderDto
{
    public string OrderId { get; set; }
    public List<OrderItemDto> Items { get; set; }
}

var order = new Order
{
    OrderId = 123,
    Items = new List<OrderItem>
    {
        new() { ProductId = 1, Quantity = 2, Price = 10.50m },
        new() { ProductId = 2, Quantity = 1, Price = 25.00m }
    }
};

var dto = mapper.Map<Order, OrderDto>(order);
// dto.OrderId = "123" (int to string conversion)
// dto.Items contains converted OrderItemDto objects
```

### Nullable Handling

```csharp
var source = new { Age = (int?)25, Name = (string?)null };
var result = mapper.Map<PersonDto>(source);
// Handles nullables correctly
```

### Case-Insensitive Property Names

```csharp
var source = new { firstname = "Alice", LASTNAME = "Smith" };
var result = mapper.Map<Person>(source);
// result.FirstName = "Alice", result.LastName = "Smith"
```

## License

MIT
