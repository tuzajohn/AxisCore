# Migration Guide from MediatR to AxisCore.Mediator

This guide helps you migrate from MediatR to AxisCore.Mediator. The libraries share similar APIs, making migration straightforward.

## Quick Overview

| Aspect | MediatR | AxisCore.Mediator |
|--------|---------|------------------|
| Return Type | `Task<T>` | `Task<T>` |
| Notification Publishing | `Task` | `Task` |
| Package Name | `MediatR` | `AxisCore.Mediator` |
| Registration | `AddMediatR()` | `AddMediator()` |

## Step-by-Step Migration

### 1. Update Package References

Remove MediatR:
```xml
<!-- Remove -->
<PackageReference Include="MediatR" Version="12.x" />
<PackageReference Include="MediatR.Extensions.Microsoft.DependencyInjection" Version="12.x" />
```

Add AxisCore.Mediator:
```xml
<!-- Add -->
<PackageReference Include="AxisCore.Mediator" Version="1.x" />
```

### 2. Update Using Directives

**Before (MediatR):**
```csharp
using MediatR;
```

**After (AxisCore.Mediator):**
```csharp
using AxisCore.Mediator;
using AxisCore.Mediator.DependencyInjection;
```

### 3. Update Service Registration

**Before (MediatR):**
```csharp
services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
```

**After (AxisCore.Mediator):**
```csharp
services.AddMediatorFromAssembly();
// Or
services.AddMediator(new[] { typeof(Program).Assembly });
```

### 4. Handler Signatures - No Changes Needed!

Handler signatures are **identical** between MediatR and AxisCore.Mediator:

```csharp
public class MyHandler : IRequestHandler<MyRequest, MyResponse>
{
    public Task<MyResponse> Handle(MyRequest request, CancellationToken cancellationToken)
    {
        var response = new MyResponse();
        return Task.FromResult(response);
    }
}
```

For async operations:
```csharp
public async Task<MyResponse> Handle(MyRequest request, CancellationToken cancellationToken)
{
    var result = await SomeAsyncOperation();
    return new MyResponse { Data = result };
}
```

### 5. Notification Handlers - No Changes Needed!

Notification handler signatures are **identical** between MediatR and AxisCore.Mediator:

```csharp
public class MyNotificationHandler : INotificationHandler<MyNotification>
{
    public Task Handle(MyNotification notification, CancellationToken cancellationToken)
    {
        // Handle notification
        return Task.CompletedTask;
    }
}
```

### 6. Pipeline Behaviors - No Changes Needed!

Pipeline behavior signatures are **identical** between MediatR and AxisCore.Mediator:

```csharp
public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Before logic
        var response = await next();
        // After logic
        return response;
    }
}
```

### 7. Update IMediator Usage

The IMediator interface is nearly identical:

**Before (MediatR):**
```csharp
var response = await _mediator.Send(new MyRequest());
await _mediator.Publish(new MyNotification());
```

**After (AxisCore.Mediator):**
```csharp
var response = await _mediator.Send(new MyRequest());
await _mediator.Publish(new MyNotification());
```

## Feature Mapping

### Request/Response

✅ **Fully Compatible** - No changes needed to request/response definitions

### Notifications

✅ **Fully Compatible** - Notification definitions remain the same

### Pipeline Behaviors

✅ **Fully Compatible** - Same return types `Task<T>`

### Pre/Post Processors

✅ **Fully Compatible** - AxisCore.Mediator supports the same pre/post processor pattern

### Streaming

✅ **Enhanced** - AxisCore.Mediator has built-in streaming support via `IStreamRequest<T>`

**MediatR approach:**
```csharp
// Custom implementation needed
```

**AxisCore.Mediator:**
```csharp
public class StreamRequest : IStreamRequest<int> { }

public class StreamHandler : IStreamRequestHandler<StreamRequest, int>
{
    public async IAsyncEnumerable<int> Handle(
        StreamRequest request,
        CancellationToken cancellationToken)
    {
        for (int i = 0; i < 10; i++)
        {
            yield return i;
        }
    }
}
```

## Configuration Options

### Notification Publishing Strategies

**AxisCore.Mediator** provides explicit control:

```csharp
services.AddMediator(options =>
{
    options.NotificationPublisherStrategy = NotificationPublisherStrategy.PublishParallel;
    // Or PublishSequential
    // Or PublishSequentialStopOnException
});
```

### Service Lifetimes

**MediatR:**
```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(assembly);
    cfg.Lifetime = ServiceLifetime.Scoped;
});
```

**AxisCore.Mediator:**
```csharp
services.AddMediator(
    new[] { assembly },
    lifetime: ServiceLifetime.Scoped);
```

## Breaking Changes & Differences

### 1. API Compatibility

Both MediatR and AxisCore.Mediator use `Task<T>` for handlers, making the APIs fully compatible. The main differences are in namespaces and registration methods.

### 2. Unit Type

**MediatR:**
```csharp
using MediatR;
// Uses MediatR.Unit
```

**AxisCore.Mediator:**
```csharp
using AxisCore.Mediator;
// Uses AxisCore.Mediator.Unit
```

Both work the same way for void requests.

### 3. Built-in Behaviors

AxisCore.Mediator includes several built-in behaviors:
- `LoggingBehavior<TRequest, TResponse>`
- `PerformanceBehavior<TRequest, TResponse>`
- `ValidationBehavior<TRequest, TResponse>`

### 4. Assembly Scanning

**MediatR:**
```csharp
services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(assembly1, assembly2));
```

**AxisCore.Mediator:**
```csharp
services.AddMediator(new[] { assembly1, assembly2 });
```

## Performance Considerations

After migration, you should see:
- **Reduced allocations** for simple request/response scenarios
- **Better performance** due to Task and handler caching
- **Lower latency** for synchronous operations

Run benchmarks to measure the improvement:
```bash
dotnet run --project benchmarks/AxisCore.Mediator.Benchmarks -c Release
```

## Common Pitfalls

### 1. Forgetting Namespace Changes

❌ **Wrong:**
```csharp
using MediatR; // Old namespace
```

✅ **Correct:**
```csharp
using AxisCore.Mediator;
using AxisCore.Mediator.DependencyInjection;
```

### 2. Unit Test Updates

Update your test mocks/stubs to use the new namespace:

```csharp
// Use AxisCore.Mediator.IMediator instead of MediatR.IMediator
_mediator.Send(Arg.Any<MyRequest>())
    .Returns(Task.FromResult(new MyResponse()));
```

## Testing the Migration

1. **Update one handler at a time** and test
2. **Run your existing test suite** to catch issues
3. **Use the compiler** - it will catch most signature mismatches
4. **Run performance benchmarks** to verify improvements

## Need Help?

- Check the [API Documentation](API.md)
- Review [examples](../samples/)
- Open an issue on [GitHub](https://github.com/tuzajohn/AxisCore/issues)

## Gradual Migration Strategy

For large codebases:

1. **Phase 1**: Install AxisCore.Mediator alongside MediatR
2. **Phase 2**: Create new handlers with AxisCore.Mediator
3. **Phase 3**: Migrate existing handlers one module at a time
4. **Phase 4**: Remove MediatR once fully migrated

## Conclusion

Migration from MediatR to AxisCore.Mediator is straightforward:
- Update package references and namespaces
- Update registration calls (AddMediatR -> AddMediator)
- Handler signatures are identical - no code changes needed!
- Enjoy built-in behaviors and enhanced streaming support!

Most changes are mechanical and can be done quickly with find-and-replace for many cases.
