namespace AxisCore.Mediator;

/// <summary>
/// Defines a middleware-style behavior for handling requests.
/// Pipeline behaviors wrap around the request handler, allowing pre/post processing.
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handle the request within the pipeline.
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="next">The next delegate in the pipeline (eventually the handler)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The response from the handler or behavior</returns>
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}

/// <summary>
/// Represents the next operation in the request pipeline.
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
/// <returns>Awaitable task returning the response</returns>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
