using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using AxisCore.Mediator.DependencyInjection;
using Xunit;

namespace AxisCore.Mediator.Tests;

public class ConcurrencyTests
{
    [Fact]
    public async Task Send_ConcurrentRequests_HandlesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddTransient<IRequestHandler<ConcurrentRequest, int>, ConcurrentRequestHandler>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var tasks = Enumerable.Range(0, 100)
            .Select(i => mediator.Send(new ConcurrentRequest { Value = i }))
            .ToArray();

        var results = await Task.WhenAll(tasks.Select(t => t.AsTask()));

        // Assert
        results.Should().HaveCount(100);
        results.Should().BeInAscendingOrder();
        results.Should().ContainInOrder(Enumerable.Range(0, 100));
    }

    [Fact]
    public async Task Publish_ConcurrentNotifications_HandlesCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddTransient<INotificationHandler<ConcurrentNotification>, ConcurrentNotificationHandler>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        ConcurrentNotificationHandler.Count = 0;

        // Act
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => mediator.Publish(new ConcurrentNotification()))
            .ToArray();

        await Task.WhenAll(tasks.Select(t => t.AsTask()));

        // Allow handlers to complete
        await Task.Delay(100);

        // Assert
        ConcurrentNotificationHandler.Count.Should().Be(100);
    }

    // Test types
    public class ConcurrentRequest : IRequest<int>
    {
        public int Value { get; set; }
    }

    public class ConcurrentRequestHandler : IRequestHandler<ConcurrentRequest, int>
    {
        public async ValueTask<int> Handle(ConcurrentRequest request, CancellationToken cancellationToken)
        {
            await Task.Delay(10, cancellationToken);
            return request.Value;
        }
    }

    public class ConcurrentNotification : INotification
    {
    }

    public class ConcurrentNotificationHandler : INotificationHandler<ConcurrentNotification>
    {
        public static int Count;

        public ValueTask Handle(ConcurrentNotification notification, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref Count);
            return ValueTask.CompletedTask;
        }
    }
}
