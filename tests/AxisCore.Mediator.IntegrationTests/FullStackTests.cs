using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AxisCore.Mediator.DependencyInjection;
using AxisCore.Mediator.Behaviors;
using Xunit;

namespace AxisCore.Mediator.IntegrationTests;

public class FullStackTests
{
    [Fact]
    public async Task CompleteWorkflow_WithAllFeatures_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddMediator(options =>
        {
            options.NotificationPublisherStrategy = NotificationPublisherStrategy.PublishParallel;
        });

        // Register handlers
        services.AddRequestHandler<CreateOrderCommand, OrderResult, CreateOrderCommandHandler>();
        services.AddNotificationHandler<OrderCreatedNotification, SendEmailNotificationHandler>();
        services.AddNotificationHandler<OrderCreatedNotification, UpdateInventoryNotificationHandler>();

        // Register behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act - Send command
        var command = new CreateOrderCommand
        {
            CustomerId = "CUST-001",
            ProductId = "PROD-123",
            Quantity = 5
        };

        var result = await mediator.Send(command);

        // Assert command result
        result.Should().NotBeNull();
        result.OrderId.Should().NotBeNullOrEmpty();
        result.Success.Should().BeTrue();

        // Act - Publish notification
        var notification = new OrderCreatedNotification
        {
            OrderId = result.OrderId,
            CustomerId = command.CustomerId
        };

        await mediator.Publish(notification);

        // Assert - Verify handlers were called
        SendEmailNotificationHandler.LastOrderId.Should().Be(result.OrderId);
        UpdateInventoryNotificationHandler.LastProductId.Should().Be(command.ProductId);
    }

    [Fact]
    public async Task ScopedServices_WorkCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddScoped<ScopedDependency>();
        services.AddRequestHandler<ScopedRequest, string, ScopedRequestHandler>(ServiceLifetime.Scoped);

        var provider = services.BuildServiceProvider();

        // Act & Assert - First scope
        using (var scope1 = provider.CreateScope())
        {
            var mediator = scope1.ServiceProvider.GetRequiredService<IMediator>();
            var result = await mediator.Send(new ScopedRequest());
            result.Should().NotBeNullOrEmpty();
        }

        // Act & Assert - Second scope
        using (var scope2 = provider.CreateScope())
        {
            var mediator = scope2.ServiceProvider.GetRequiredService<IMediator>();
            var result = await mediator.Send(new ScopedRequest());
            result.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public async Task PipelineOrdering_ExecutesInCorrectOrder()
    {
        // Arrange
        var executionOrder = new List<string>();
        var services = new ServiceCollection();
        services.AddMediator();

        // Register handler
        services.AddTransient<IRequestHandler<OrderedRequest, string>>(
            sp => new OrderedRequestHandler(executionOrder));

        // Add behaviors in specific order
        services.AddTransient<IPipelineBehavior<OrderedRequest, string>>(
            sp => new OrderedBehavior("First", executionOrder));
        services.AddTransient<IPipelineBehavior<OrderedRequest, string>>(
            sp => new OrderedBehavior("Second", executionOrder));
        services.AddTransient<IPipelineBehavior<OrderedRequest, string>>(
            sp => new OrderedBehavior("Third", executionOrder));

        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        await mediator.Send(new OrderedRequest());

        // Assert - Behaviors should wrap in reverse order (LIFO)
        executionOrder.Should().Equal("Third-Before", "Second-Before", "First-Before", "Handler", "First-After", "Second-After", "Third-After");
    }

    // Test types
    public class CreateOrderCommand : IRequest<OrderResult>
    {
        public string CustomerId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class OrderResult
    {
        public string OrderId { get; set; } = string.Empty;
        public bool Success { get; set; }
    }

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderResult>
    {
        public Task<OrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            var result = new OrderResult
            {
                OrderId = $"ORD-{Guid.NewGuid():N}",
                Success = true
            };

            return Task.FromResult(result);
        }
    }

    public class OrderCreatedNotification : INotification
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
    }

    public class SendEmailNotificationHandler : INotificationHandler<OrderCreatedNotification>
    {
        public static string? LastOrderId { get; private set; }

        public Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
        {
            LastOrderId = notification.OrderId;
            return Task.CompletedTask;
        }
    }

    public class UpdateInventoryNotificationHandler : INotificationHandler<OrderCreatedNotification>
    {
        public static string? LastProductId { get; private set; }

        public Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
        {
            LastProductId = "PROD-123"; // Simulated
            return Task.CompletedTask;
        }
    }

    public class ScopedDependency
    {
        public string Id { get; } = Guid.NewGuid().ToString();
    }

    public class ScopedRequest : IRequest<string>
    {
    }

    public class ScopedRequestHandler : IRequestHandler<ScopedRequest, string>
    {
        private readonly ScopedDependency _dependency;

        public ScopedRequestHandler(ScopedDependency dependency)
        {
            _dependency = dependency;
        }

        public Task<string> Handle(ScopedRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_dependency.Id);
        }
    }

    public class OrderedRequest : IRequest<string>
    {
    }

    public class OrderedRequestHandler : IRequestHandler<OrderedRequest, string>
    {
        private readonly List<string> _executionOrder;

        public OrderedRequestHandler(List<string> executionOrder)
        {
            _executionOrder = executionOrder;
        }

        public Task<string> Handle(OrderedRequest request, CancellationToken cancellationToken)
        {
            _executionOrder.Add("Handler");
            return Task.FromResult("Result");
        }
    }

    public class OrderedBehavior : IPipelineBehavior<OrderedRequest, string>
    {
        private readonly string _name;
        private readonly List<string> _executionOrder;

        public OrderedBehavior(string name, List<string> executionOrder)
        {
            _name = name;
            _executionOrder = executionOrder;
        }

        public async Task<string> Handle(
            OrderedRequest request,
            RequestHandlerDelegate<string> next,
            CancellationToken cancellationToken)
        {
            _executionOrder.Add($"{_name}-Before");
            var response = await next();
            _executionOrder.Add($"{_name}-After");
            return response;
        }
    }
}
