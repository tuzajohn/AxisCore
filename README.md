# AxisCore

[![CI/CD](https://github.com/tuzajohn/AxisCore/actions/workflows/ci.yml/badge.svg)](https://github.com/tuzajohn/AxisCore/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

High-performance .NET libraries for building modern, efficient applications. AxisCore provides production-ready implementations of common patterns with a focus on performance, minimal dependencies, and developer experience.

## Libraries

### 🚀 AxisCore.Mediator

[![NuGet](https://img.shields.io/nuget/v/AxisCore.Mediator.svg)](https://www.nuget.org/packages/AxisCore.Mediator/)

A high-performance mediator pattern implementation providing request/response and pub/sub messaging for decoupled architectures.

**Features:**
- Task-based async APIs
- Pipeline behaviors for cross-cutting concerns
- Request/response, pub/sub, and streaming patterns
- Minimal dependencies (only Microsoft.Extensions abstractions)
- Multi-targeting: .NET 6, 8, and 9

**Quick Start:**
```csharp
// Setup
services.AddMediator();

// Define request and handler
public record GetUserQuery(int Id) : IRequest<User>;

public class GetUserHandler : IRequestHandler<GetUserQuery, User>
{
    public async Task<User> Handle(GetUserQuery request, CancellationToken ct)
    {
        return await _repository.GetUserAsync(request.Id, ct);
    }
}

// Use it
var user = await _mediator.Send(new GetUserQuery(123));
```

---

### 🔄 AxisCore.Mapper

A high-performance object mapper with automatic type conversion, collection handling, and compiled expressions.

**Features:**
- Automatic type conversion (int ↔ string, primitives, etc.)
- Collection mapping (List, Array, Dictionary)
- Nullable type handling
- Case-insensitive property matching
- Custom mapping configuration
- Compiled expression caching for performance
- Multi-targeting: .NET 6, 8, and 9

**Quick Start:**
```csharp
// Setup
services.AddMapper();

// Use it
var source = new Person { Name = "John", Age = 30 };
var dto = _mapper.Map<PersonDto>(source);

// Automatic type conversion
var obj = new { Count = 42 };
var result = _mapper.Map<WithString>(obj);
// result.Count = "42" (int to string)

// Collections with type conversion
var nums = new { Values = new[] { 1, 2, 3 } };
var mapped = _mapper.Map<WithStringList>(nums);
// mapped.Values = ["1", "2", "3"]

// Invalid conversion throws exception
var bad = new { Age = "hello" };
_mapper.Map<WithInt>(bad); // Throws: Cannot convert 'hello' to int
```

[View AxisCore.Mapper Documentation →](src/AxisCore.Mapper/README.md)

---

## Installation

Install via NuGet:

```bash
# Mediator
dotnet add package AxisCore.Mediator

# Mapper
dotnet add package AxisCore.Mapper

# Or both
dotnet add package AxisCore.Mediator
dotnet add package AxisCore.Mapper
```

## Design Philosophy

AxisCore libraries are built with the following principles:

- **Performance First**: Compiled expressions, caching, and optimized async patterns
- **Minimal Dependencies**: Only essential Microsoft.Extensions abstractions
- **Developer Experience**: Clean APIs, comprehensive documentation, and extensive testing
- **Production Ready**: Multi-targeting, error handling, and battle-tested patterns
- **Type Safety**: Leveraging C# type system for compile-time safety

## Target Frameworks

All libraries support:
- .NET 9 (latest)
- .NET 8 (LTS)
- .NET 6 (LTS)

## Project Structure

```
AxisCore/
├── src/
│   ├── AxisCore.Mediator/          # Mediator pattern implementation
│   └── AxisCore.Mapper/            # Object mapper implementation
├── tests/
│   ├── AxisCore.Mediator.Tests/
│   ├── AxisCore.Mediator.IntegrationTests/
│   └── AxisCore.Mapper.UnitTests/
├── benchmarks/
│   └── AxisCore.Mediator.Benchmarks/
└── samples/
    └── BasicUsage/
```

## Building from Source

```bash
# Clone the repository
git clone https://github.com/tuzajohn/AxisCore.git
cd AxisCore

# Build the solution
dotnet build

# Run tests
dotnet test

# Run benchmarks
dotnet run --project benchmarks/AxisCore.Mediator.Benchmarks -c Release
```

## Dependencies

All AxisCore libraries have minimal external dependencies:

- **AxisCore.Mediator**:
  - Microsoft.Extensions.DependencyInjection.Abstractions
  - Microsoft.Extensions.Logging.Abstractions

- **AxisCore.Mapper**:
  - Microsoft.Extensions.DependencyInjection.Abstractions

## Using Both Libraries Together

```csharp
// Configure services
services.AddMediator();
services.AddMapper();

// Handler that uses mapper
public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IUserRepository _repository;
    private readonly IMapper _mapper;

    public CreateUserHandler(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(CreateUserCommand command, CancellationToken ct)
    {
        // Map command to entity
        var user = _mapper.Map<User>(command);

        // Save to repository
        await _repository.AddAsync(user, ct);

        // Map entity to DTO
        return _mapper.Map<UserDto>(user);
    }
}

// Use in application
var command = new CreateUserCommand { Name = "John Doe", Email = "john@example.com" };
var userDto = await _mediator.Send(command);
```

## Performance

AxisCore libraries are designed for high performance:

- **Compiled Expressions**: Handler resolution and mapping logic compiled at runtime
- **Caching**: Type mappings and handler delegates cached to avoid reflection overhead
- **Async/Await**: Fully asynchronous APIs with proper cancellation support
- **Minimal Allocations**: Careful design to reduce GC pressure

### AxisCore.Mediator Benchmarks

| Method           | Mean     | Error   | Allocated |
|-----------------|----------|---------|-----------|
| AxisCore_Send    | 45.2 ns  | 0.4 ns  | 0 B       |
| AxisCore_Publish | 123.1 ns | 2.1 ns  | 0 B       |

Run the benchmarks:

```bash
cd benchmarks/AxisCore.Mediator.Benchmarks
dotnet run -c Release
```

## Examples and Documentation

### AxisCore.Mediator

- [Full Documentation](docs/API.md)
- [Migration from MediatR](docs/MIGRATION.md)
- [Performance Guide](docs/PERFORMANCE.md)
- [Best Practices](docs/BEST_PRACTICES.md)
- [Basic Usage Sample](samples/BasicUsage/)

### AxisCore.Mapper

- [Full Documentation](src/AxisCore.Mapper/README.md)

## Advanced Scenarios

### Pipeline Behavior with Mapping

```csharp
public class MappingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMapper _mapper;

    public MappingBehavior(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Pre-processing with mapping
        var normalized = _mapper.Map<NormalizedRequest>(request);

        // Continue pipeline
        var response = await next();

        // Post-processing with mapping
        return _mapper.Map<TResponse>(response);
    }
}
```

### Validation with Type Conversion

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Validate before processing
        ValidateRequest(request);

        return await next();
    }

    private void ValidateRequest(TRequest request)
    {
        // Validation logic
        if (request == null)
            throw new ArgumentNullException(nameof(request));
    }
}

// Register
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for bugs and feature requests.

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## Support

- **Documentation**: See individual library README files
- **Issues**: [GitHub Issues](https://github.com/tuzajohn/AxisCore/issues)
- **Discussions**: [GitHub Discussions](https://github.com/tuzajohn/AxisCore/discussions)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Roadmap

- [x] AxisCore.Mediator - High-performance mediator pattern
- [x] AxisCore.Mapper - High-performance object mapper
- [ ] AxisCore.Validation - Fluent validation library
- [ ] Source generators for compile-time handler registration
- [ ] Additional collection types support
- [ ] Expression-based mapping configuration

## Acknowledgments

- **AxisCore.Mediator** is inspired by [MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard
- **AxisCore.Mapper** is inspired by [AutoMapper](https://github.com/AutoMapper/AutoMapper)

Both libraries aim to provide similar developer experiences with enhanced performance characteristics.

---

**Built with ❤️ for the .NET community**
