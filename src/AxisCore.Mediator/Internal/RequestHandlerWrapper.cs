using Microsoft.Extensions.DependencyInjection;

namespace AxisCore.Mediator;

/// <summary>
/// Base wrapper for request handlers to enable caching and performance optimizations.
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
internal abstract class RequestHandlerWrapper<TResponse>
{
    public abstract ValueTask<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken);
}

/// <summary>
/// Concrete implementation of request handler wrapper.
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
internal sealed class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public override async ValueTask<TResponse> Handle(
        IRequest<TResponse> request,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var typedRequest = (TRequest)request;

        // Get pre-processors
        var preProcessors = serviceProvider.GetServices<IRequestPreProcessor<TRequest>>();

        // Execute pre-processors
        foreach (var preProcessor in preProcessors)
        {
            await preProcessor.Process(typedRequest, cancellationToken).ConfigureAwait(false);
        }

        // Get pipeline behaviors
        var behaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, TResponse>>().ToArray();

        // Build the handler pipeline
        RequestHandlerDelegate<TResponse> handler = async () =>
        {
            var requestHandler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
            return await requestHandler.Handle(typedRequest, cancellationToken).ConfigureAwait(false);
        };

        // Wrap with behaviors in reverse order (iterate forward to apply in LIFO)
        foreach (var behavior in behaviors.Reverse())
        {
            handler = WrapBehavior(behavior, handler, typedRequest, cancellationToken);
        }

        static RequestHandlerDelegate<TResponse> WrapBehavior(
            IPipelineBehavior<TRequest, TResponse> behavior,
            RequestHandlerDelegate<TResponse> next,
            TRequest request,
            CancellationToken cancellationToken)
        {
            return () => behavior.Handle(request, next, cancellationToken);
        }

        // Execute the pipeline
        var response = await handler().ConfigureAwait(false);

        // Get post-processors
        var postProcessors = serviceProvider.GetServices<IRequestPostProcessor<TRequest, TResponse>>();

        // Execute post-processors
        foreach (var postProcessor in postProcessors)
        {
            await postProcessor.Process(typedRequest, response, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }
}
