using Microsoft.Extensions.DependencyInjection;

namespace Conduit.Mediator;

/// <summary>
/// Base wrapper for stream request handlers.
/// </summary>
/// <typeparam name="TResponse">Stream item type</typeparam>
internal abstract class StreamHandlerWrapper<TResponse>
{
    public abstract IAsyncEnumerable<TResponse> Handle(
        IStreamRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

/// <summary>
/// Concrete implementation of stream handler wrapper.
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Stream item type</typeparam>
internal sealed class StreamHandlerWrapperImpl<TRequest, TResponse> : StreamHandlerWrapper<TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    public override IAsyncEnumerable<TResponse> Handle(
        IStreamRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IStreamRequestHandler<TRequest, TResponse>>();
        return handler.Handle((TRequest)request, cancellationToken);
    }
}
