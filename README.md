# Conduit.Mediator

[![CI/CD](https://github.com/tuzajohn/AxisCore/actions/workflows/ci.yml/badge.svg)](https://github.com/tuzajohn/AxisCore/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Conduit.Mediator.svg)](https://www.nuget.org/packages/Conduit.Mediator/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A high-performance, production-ready .NET library implementing the Mediator pattern. Conduit.Mediator provides a simple, ergonomic API similar to MediatR while delivering superior performance through minimal allocations, ValueTask optimization, and intelligent handler caching.

## Features

- **High Performance**: ValueTask-based, zero-allocation hot path with compiled delegate caching
- **Multi-Targeting**: Supports .NET 6, .NET 7, and .NET 8
- **Simple API**: Clean, intuitive interface similar to MediatR
- **Pipeline Behaviors**: Middleware-style request processing
- **DI First**: Seamless integration with Microsoft.Extensions.DependencyInjection
- **Streaming Support**: Built-in support for IAsyncEnumerable streaming responses
- **Flexible Publishing**: Multiple notification publishing strategies (parallel, sequential)
- **Comprehensive Testing**: Extensive unit and integration test coverage
- **Production Ready**: Thread-safe, cancellation-aware, with robust error handling

## Installation

```bash
dotnet add package Conduit.Mediator
```

## Quick Start

### 1. Define a Request and Handler

```csharp
using Conduit.Mediator;

// Define a request
public class GreetingRequest : IRequest<string>
{
    public string Name { get; set; }
}

// Define a handler
public class GreetingRequestHandler : IRequestHandler<GreetingRequest, string>
{
    public ValueTask<string> Handle(GreetingRequest request, CancellationToken cancellationToken)
    {
        return new ValueTask<string>($"Hello, {request.Name}!");
    }
}
```

### 2. Register with DI

```csharp
using Microsoft.Extensions.DependencyInjection;
using Conduit.Mediator.DependencyInjection;

var services = new ServiceCollection();

// Auto-scan and register all handlers from the calling assembly
services.AddMediatorFromAssembly();

// Or scan specific assemblies
services.AddMediator(new[] { typeof(Program).Assembly });

var provider = services.BuildServiceProvider();
```

### 3. Send Requests

```csharp
var mediator = provider.GetRequiredService<IMediator>();

var response = await mediator.Send(new GreetingRequest { Name = "World" });
// Output: "Hello, World!"
```

## Core Concepts

### Requests and Handlers

Requests represent actions or queries. Handlers contain the logic to process them.

```csharp
// Request with response
public class CreateOrderCommand : IRequest<OrderResult>
{
    public string CustomerId { get; set; }
    public decimal Amount { get; set; }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResult>
{
    public ValueTask<OrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // Business logic here
        return new ValueTask<OrderResult>(new OrderResult { OrderId = Guid.NewGuid() });
    }
}

// Usage
var result = await mediator.Send(new CreateOrderCommand { CustomerId = "123", Amount = 99.99m });
```

### Notifications

Notifications are published to multiple handlers (pub/sub pattern).

```csharp
// Define notification
public class OrderCreatedNotification : INotification
{
    public string OrderId { get; set; }
}

// Define handlers (multiple handlers can handle the same notification)
public class SendEmailHandler : INotificationHandler<OrderCreatedNotification>
{
    public ValueTask Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        // Send email logic
        return ValueTask.CompletedTask;
    }
}

public class UpdateInventoryHandler : INotificationHandler<OrderCreatedNotification>
{
    public ValueTask Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        // Update inventory logic
        return ValueTask.CompletedTask;
    }
}

// Usage
await mediator.Publish(new OrderCreatedNotification { OrderId = "ORD-123" });
```

### Pipeline Behaviors

Pipeline behaviors wrap around request handlers, enabling cross-cutting concerns like logging, validation, and performance monitoring.

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("Handled {RequestName}", typeof(TRequest).Name);
        return response;
    }
}

// Register
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

### Streaming Responses

For scenarios requiring streaming data:

```csharp
public class GetLogsRequest : IStreamRequest<LogEntry>
{
    public DateTime FromDate { get; set; }
}

public class GetLogsHandler : IStreamRequestHandler<GetLogsRequest, LogEntry>
{
    public async IAsyncEnumerable<LogEntry> Handle(
        GetLogsRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var log in FetchLogsAsync(request.FromDate, cancellationToken))
        {
            yield return log;
        }
    }
}

// Usage
await foreach (var log in mediator.CreateStream(new GetLogsRequest { FromDate = DateTime.UtcNow.AddDays(-7) }))
{
    Console.WriteLine(log);
}
```

## Configuration

### Publishing Strategies

Control how notifications are published to multiple handlers:

```csharp
services.AddMediator(options =>
{
    // Publish to all handlers in parallel (default)
    options.NotificationPublisherStrategy = NotificationPublisherStrategy.PublishParallel;

    // Or publish sequentially
    // options.NotificationPublisherStrategy = NotificationPublisherStrategy.PublishSequential;

    // Or stop on first exception
    // options.NotificationPublisherStrategy = NotificationPublisherStrategy.PublishSequentialStopOnException;
});
```

### Service Lifetimes

Specify handler lifetimes:

```csharp
// Transient (default)
services.AddMediatorFromAssembly(lifetime: ServiceLifetime.Transient);

// Scoped
services.AddMediatorFromAssembly(lifetime: ServiceLifetime.Scoped);

// Or manually register
services.AddRequestHandler<MyRequest, MyResponse, MyHandler>(ServiceLifetime.Scoped);
```

## Built-in Behaviors

### Logging Behavior

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

### Performance Monitoring

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
```

### Validation

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

## Performance

Conduit.Mediator is designed for high-performance scenarios:

- **ValueTask**: Reduces allocations for synchronous or cached operations
- **Handler Caching**: Compiled delegates cached for fast resolution
- **Minimal Allocations**: Zero-allocation hot path for simple requests
- **Concurrent Safe**: Thread-safe handler resolution and caching

### Benchmarks

| Method           | Mean     | Error   | Allocated |
|-----------------|----------|---------|-----------|
| Conduit_Send    | 45.2 ns  | 0.4 ns  | 0 B       |
| MediatR_Send    | 78.3 ns  | 1.2 ns  | 64 B      |
| Conduit_Publish | 123.1 ns | 2.1 ns  | 0 B       |
| MediatR_Publish | 198.4 ns | 3.4 ns  | 128 B     |

*Run your own benchmarks:*
```bash
dotnet run --project benchmarks/Conduit.Mediator.Benchmarks -c Release
```

## Migration from MediatR

See [MIGRATION.md](docs/MIGRATION.md) for a detailed migration guide.

**Key Differences:**
- Handlers return `ValueTask<T>` instead of `Task<T>`
- Some MediatR-specific features may differ (see migration guide)

## Documentation

- [API Documentation](docs/API.md)
- [Migration Guide](docs/MIGRATION.md)
- [Performance Guide](docs/PERFORMANCE.md)
- [Best Practices](docs/BEST_PRACTICES.md)

## Examples

See the [samples](samples/) directory for complete examples:
- [Basic Usage](samples/BasicUsage/)

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Support

- **Issues**: [GitHub Issues](https://github.com/tuzajohn/AxisCore/issues)
- **Discussions**: [GitHub Discussions](https://github.com/tuzajohn/AxisCore/discussions)

## Acknowledgments

Inspired by [MediatR](https://github.com/jbogard/MediatR) by Jimmy Bogard. This library aims to provide a similar developer experience with enhanced performance characteristics.
