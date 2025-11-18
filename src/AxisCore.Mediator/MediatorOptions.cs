namespace AxisCore.Mediator;

/// <summary>
/// Configuration options for the mediator.
/// </summary>
public sealed class MediatorOptions
{
    /// <summary>
    /// Gets or sets the notification publisher strategy.
    /// Default is <see cref="NotificationPublisherStrategy.PublishParallel"/>.
    /// </summary>
    public NotificationPublisherStrategy NotificationPublisherStrategy { get; set; } = NotificationPublisherStrategy.PublishParallel;

    /// <summary>
    /// Gets or sets whether to allow handler inheritance (handlers for base types handle derived types).
    /// Default is false for performance.
    /// </summary>
    public bool AllowHandlerInheritance { get; set; } = false;
}

/// <summary>
/// Strategy for publishing notifications to multiple handlers.
/// </summary>
public enum NotificationPublisherStrategy
{
    /// <summary>
    /// Publish to all handlers in parallel (using Task.WhenAll).
    /// </summary>
    PublishParallel,

    /// <summary>
    /// Publish to all handlers sequentially in registration order.
    /// </summary>
    PublishSequential,

    /// <summary>
    /// Publish to handlers sequentially, stopping on first exception.
    /// </summary>
    PublishSequentialStopOnException
}
