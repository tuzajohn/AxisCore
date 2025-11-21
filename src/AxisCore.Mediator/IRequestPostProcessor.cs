namespace AxisCore.Mediator;

/// <summary>
/// Defines a processor that runs after the request handler.
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IRequestPostProcessor<in TRequest, in TResponse>
{
    /// <summary>
    /// Process method executes after the request handler.
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="response">The response from the handler</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task Process(TRequest request, TResponse response, CancellationToken cancellationToken);
}
