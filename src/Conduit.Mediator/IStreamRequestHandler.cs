namespace Conduit.Mediator;

/// <summary>
/// Defines a handler for a streaming request.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
/// <typeparam name="TResponse">The type of items in the response stream</typeparam>
public interface IStreamRequestHandler<in TRequest, out TResponse>
    where TRequest : IStreamRequest<TResponse>
{
    /// <summary>
    /// Handles a streaming request.
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Async enumerable stream of responses</returns>
    IAsyncEnumerable<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
