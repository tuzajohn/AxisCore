using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Conduit.Mediator;

/// <summary>
/// Default implementation of <see cref="IMediator"/>.
/// Dispatches requests to handlers with pipeline behavior support.
/// </summary>
public sealed class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MediatorOptions _options;

    private static readonly ConcurrentDictionary<Type, object> _requestHandlerCache = new();
    private static readonly ConcurrentDictionary<Type, object> _notificationHandlersCache = new();
    private static readonly ConcurrentDictionary<Type, object> _streamHandlerCache = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Mediator"/> class.
    /// </summary>
    /// <param name="serviceProvider">Service provider for resolving handlers</param>
    /// <param name="options">Mediator options</param>
    public Mediator(IServiceProvider serviceProvider, MediatorOptions options)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public async ValueTask<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var requestType = request.GetType();
        var handler = GetRequestHandler<TResponse>(requestType);

        return await handler.Handle(request, _serviceProvider, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public ValueTask Publish<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        if (notification is null)
        {
            throw new ArgumentNullException(nameof(notification));
        }

        return PublishCore(notification, cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask Publish(
        object notification,
        CancellationToken cancellationToken = default)
    {
        if (notification is null)
        {
            throw new ArgumentNullException(nameof(notification));
        }

        if (notification is INotification notificationInstance)
        {
            return PublishNotification(notificationInstance, cancellationToken);
        }

        throw new ArgumentException($"Notification does not implement {nameof(INotification)}", nameof(notification));
    }

    /// <inheritdoc />
    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(
        IStreamRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var requestType = request.GetType();
        var handler = GetStreamHandler<TResponse>(requestType);

        return handler.Handle(request, _serviceProvider, cancellationToken);
    }

    private async ValueTask PublishCore<TNotification>(
        TNotification notification,
        CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var notificationType = notification.GetType();
        var handlers = GetNotificationHandlers<TNotification>(notificationType);

        await handlers.Handle(notification, _serviceProvider, _options.NotificationPublisherStrategy, cancellationToken)
            .ConfigureAwait(false);
    }

    private ValueTask PublishNotification(
        INotification notification,
        CancellationToken cancellationToken)
    {
        var notificationType = notification.GetType();
        var method = typeof(Mediator)
            .GetMethod(nameof(PublishCore), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .MakeGenericMethod(notificationType);

        var task = method.Invoke(this, new object[] { notification, cancellationToken });
        return (ValueTask)task!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RequestHandlerWrapper<TResponse> GetRequestHandler<TResponse>(Type requestType)
    {
        return (RequestHandlerWrapper<TResponse>)_requestHandlerCache.GetOrAdd(
            requestType,
            static (type, mediator) => CreateRequestHandler<TResponse>(type),
            this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private NotificationHandlersWrapper<TNotification> GetNotificationHandlers<TNotification>(Type notificationType)
        where TNotification : INotification
    {
        return (NotificationHandlersWrapper<TNotification>)_notificationHandlersCache.GetOrAdd(
            notificationType,
            static type => CreateNotificationHandlers<TNotification>(type));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private StreamHandlerWrapper<TResponse> GetStreamHandler<TResponse>(Type requestType)
    {
        return (StreamHandlerWrapper<TResponse>)_streamHandlerCache.GetOrAdd(
            requestType,
            static type => CreateStreamHandler<TResponse>(type));
    }

    private static RequestHandlerWrapper<TResponse> CreateRequestHandler<TResponse>(Type requestType)
    {
        var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse));
        var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {requestType}");
        return (RequestHandlerWrapper<TResponse>)wrapper;
    }

    private static NotificationHandlersWrapper<TNotification> CreateNotificationHandlers<TNotification>(Type notificationType)
        where TNotification : INotification
    {
        var wrapperType = typeof(NotificationHandlersWrapperImpl<>).MakeGenericType(notificationType);
        var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {notificationType}");
        return (NotificationHandlersWrapper<TNotification>)wrapper;
    }

    private static StreamHandlerWrapper<TResponse> CreateStreamHandler<TResponse>(Type requestType)
    {
        var wrapperType = typeof(StreamHandlerWrapperImpl<,>).MakeGenericType(requestType, typeof(TResponse));
        var wrapper = Activator.CreateInstance(wrapperType) ?? throw new InvalidOperationException($"Could not create wrapper type for {requestType}");
        return (StreamHandlerWrapper<TResponse>)wrapper;
    }
}
