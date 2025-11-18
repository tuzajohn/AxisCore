namespace Conduit.Mediator;

/// <summary>
/// Defines a mediator to dispatch requests and publish notifications.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Send a request to a single handler.
    /// </summary>
    /// <typeparam name="TResponse">Response type</typeparam>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the send operation. The task result contains the handler response.</returns>
    ValueTask<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Publish a notification to multiple handlers.
    /// </summary>
    /// <typeparam name="TNotification">Notification type</typeparam>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation</returns>
    ValueTask Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification;

    /// <summary>
    /// Publish a notification object to multiple handlers.
    /// </summary>
    /// <param name="notification">Notification object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>A task that represents the publish operation</returns>
    ValueTask Publish(object notification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a stream from a request.
    /// </summary>
    /// <typeparam name="TResponse">Stream item type</typeparam>
    /// <param name="request">Request object</param>
    /// <param name="cancellationToken">Optional cancellation token</param>
    /// <returns>An async enumerable of response items</returns>
    IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default);
}
