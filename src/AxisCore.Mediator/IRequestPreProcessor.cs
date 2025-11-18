namespace AxisCore.Mediator;

/// <summary>
/// Defines a processor that runs before the request handler.
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
public interface IRequestPreProcessor<in TRequest>
{
    /// <summary>
    /// Process method executes before the request handler.
    /// </summary>
    /// <param name="request">The request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    ValueTask Process(TRequest request, CancellationToken cancellationToken);
}
