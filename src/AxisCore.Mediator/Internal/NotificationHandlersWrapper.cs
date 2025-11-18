using Microsoft.Extensions.DependencyInjection;

namespace AxisCore.Mediator;

/// <summary>
/// Base wrapper for notification handlers.
/// </summary>
/// <typeparam name="TNotification">Notification type</typeparam>
internal abstract class NotificationHandlersWrapper<TNotification>
    where TNotification : INotification
{
    public abstract ValueTask Handle(
        TNotification notification,
        IServiceProvider serviceProvider,
        NotificationPublisherStrategy strategy,
        CancellationToken cancellationToken);
}

/// <summary>
/// Concrete implementation of notification handlers wrapper.
/// </summary>
/// <typeparam name="TNotification">Notification type</typeparam>
internal sealed class NotificationHandlersWrapperImpl<TNotification> : NotificationHandlersWrapper<TNotification>
    where TNotification : INotification
{
    public override async ValueTask Handle(
        TNotification notification,
        IServiceProvider serviceProvider,
        NotificationPublisherStrategy strategy,
        CancellationToken cancellationToken)
    {
        var handlers = serviceProvider.GetServices<INotificationHandler<TNotification>>();

        switch (strategy)
        {
            case NotificationPublisherStrategy.PublishParallel:
                await PublishParallel(notification, handlers, cancellationToken).ConfigureAwait(false);
                break;

            case NotificationPublisherStrategy.PublishSequential:
                await PublishSequential(notification, handlers, cancellationToken, continueOnException: true).ConfigureAwait(false);
                break;

            case NotificationPublisherStrategy.PublishSequentialStopOnException:
                await PublishSequential(notification, handlers, cancellationToken, continueOnException: false).ConfigureAwait(false);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(strategy), strategy, "Unknown notification publisher strategy");
        }
    }

    private static async ValueTask PublishParallel(
        TNotification notification,
        IEnumerable<INotificationHandler<TNotification>> handlers,
        CancellationToken cancellationToken)
    {
        var tasks = handlers.Select(h => h.Handle(notification, cancellationToken).AsTask()).ToArray();

        if (tasks.Length == 0)
        {
            return;
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private static async ValueTask PublishSequential(
        TNotification notification,
        IEnumerable<INotificationHandler<TNotification>> handlers,
        CancellationToken cancellationToken,
        bool continueOnException)
    {
        List<Exception>? exceptions = continueOnException ? new List<Exception>() : null;

        foreach (var handler in handlers)
        {
            try
            {
                await handler.Handle(notification, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (continueOnException)
            {
                exceptions!.Add(ex);
            }
        }

        if (exceptions?.Count > 0)
        {
            throw new AggregateException("One or more notification handlers threw an exception", exceptions);
        }
    }
}
