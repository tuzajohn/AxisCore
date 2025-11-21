using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AxisCore.Mediator;
using AxisCore.Mediator.DependencyInjection;
using AxisCore.Mediator.Behaviors;

namespace BasicUsage;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Setup DI container
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Add AxisCore.Mediator and scan this assembly for handlers
        services.AddMediatorFromAssembly(options =>
        {
            options.NotificationPublisherStrategy = NotificationPublisherStrategy.PublishParallel;
        });

        // Add pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var logger = provider.GetRequiredService<ILogger<Program>>();

        logger.LogInformation("=== AxisCore.Mediator Sample Application ===\n");

        // Example 1: Simple Request/Response
        logger.LogInformation("Example 1: Simple Request/Response");
        var greetingRequest = new GreetingRequest { Name = "World" };
        var greetingResponse = await mediator.Send(greetingRequest);
        logger.LogInformation("Response: {Response}\n", greetingResponse);

        // Example 2: Command with business logic
        logger.LogInformation("Example 2: Create Order Command");
        var createOrderCommand = new CreateOrderCommand
        {
            CustomerId = "CUST-001",
            ProductId = "PROD-123",
            Quantity = 5
        };
        var orderResult = await mediator.Send(createOrderCommand);
        logger.LogInformation("Order created: {OrderId}, Total: ${Total:F2}\n", orderResult.OrderId, orderResult.TotalAmount);

        // Example 3: Notifications (multiple handlers)
        logger.LogInformation("Example 3: Publishing Notification");
        var orderCreatedNotification = new OrderCreatedNotification
        {
            OrderId = orderResult.OrderId,
            CustomerId = createOrderCommand.CustomerId,
            TotalAmount = orderResult.TotalAmount
        };
        await mediator.Publish(orderCreatedNotification);
        logger.LogInformation("Notification published to all handlers\n");

        // Example 4: Streaming response
        logger.LogInformation("Example 4: Streaming Response");
        var streamRequest = new NumberStreamRequest { Count = 5 };
        await foreach (var number in mediator.CreateStream(streamRequest))
        {
            logger.LogInformation("Received: {Number}", number);
        }

        logger.LogInformation("\n=== Sample Application Complete ===");
    }
}

// === Request/Response Examples ===

public class GreetingRequest : IRequest<string>
{
    public string Name { get; set; } = string.Empty;
}

public class GreetingRequestHandler : IRequestHandler<GreetingRequest, string>
{
    public Task<string> Handle(GreetingRequest request, CancellationToken cancellationToken)
    {
        return Task.FromResult($"Hello, {request.Name}!");
    }
}

// === Command Example ===

public class CreateOrderCommand : IRequest<OrderResult>
{
    public string CustomerId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class OrderResult
{
    public string OrderId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResult>
{
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(ILogger<CreateOrderCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task<OrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating order for customer {CustomerId}", request.CustomerId);

        // Simulate business logic
        var pricePerUnit = 29.99m;
        var totalAmount = pricePerUnit * request.Quantity;

        var result = new OrderResult
        {
            OrderId = $"ORD-{Guid.NewGuid():N}",
            TotalAmount = totalAmount
        };

        return Task.FromResult(result);
    }
}

// === Notification Examples ===

public class OrderCreatedNotification : INotification
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}

public class SendEmailNotificationHandler : INotificationHandler<OrderCreatedNotification>
{
    private readonly ILogger<SendEmailNotificationHandler> _logger;

    public SendEmailNotificationHandler(ILogger<SendEmailNotificationHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending email for order {OrderId} to customer {CustomerId}",
            notification.OrderId, notification.CustomerId);
        return Task.CompletedTask;
    }
}

public class UpdateInventoryNotificationHandler : INotificationHandler<OrderCreatedNotification>
{
    private readonly ILogger<UpdateInventoryNotificationHandler> _logger;

    public UpdateInventoryNotificationHandler(ILogger<UpdateInventoryNotificationHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating inventory for order {OrderId}", notification.OrderId);
        return Task.CompletedTask;
    }
}

// === Streaming Example ===

public class NumberStreamRequest : IStreamRequest<int>
{
    public int Count { get; set; }
}

public class NumberStreamRequestHandler : IStreamRequestHandler<NumberStreamRequest, int>
{
    public async IAsyncEnumerable<int> Handle(
        NumberStreamRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        for (int i = 1; i <= request.Count; i++)
        {
            await Task.Delay(100, cancellationToken);
            yield return i;
        }
    }
}
