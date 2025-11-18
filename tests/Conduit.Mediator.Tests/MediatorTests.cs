using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Conduit.Mediator.DependencyInjection;
using Xunit;

namespace Conduit.Mediator.Tests;

public class MediatorTests
{
    [Fact]
    public async Task Send_WithValidRequest_ReturnsResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new TestRequest { Value = "test" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        result.Should().Be("Handled: test");
    }

    [Fact]
    public async Task Send_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            mediator.Send<string>(null!).AsTask());
    }

    [Fact]
    public async Task Send_WithCancellationToken_PropagatesToken()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddTransient<IRequestHandler<CancellableRequest, string>, CancellableRequestHandler>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            mediator.Send(new CancellableRequest(), cts.Token).AsTask());
    }

    [Fact]
    public async Task Send_WithPipelineBehavior_ExecutesBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        services.AddTransient<IPipelineBehavior<TestRequest, string>, TestPipelineBehavior>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new TestRequest { Value = "test" };

        // Act
        var result = await mediator.Send(request);

        // Assert
        result.Should().Be("Before -> Handled: test -> After");
    }

    [Fact]
    public async Task Publish_WithNotification_CallsAllHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddTransient<INotificationHandler<TestNotification>, TestNotificationHandler1>();
        services.AddTransient<INotificationHandler<TestNotification>, TestNotificationHandler2>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var notification = new TestNotification();

        TestNotificationHandler1.Called = false;
        TestNotificationHandler2.Called = false;

        // Act
        await mediator.Publish(notification);

        // Assert
        TestNotificationHandler1.Called.Should().BeTrue();
        TestNotificationHandler2.Called.Should().BeTrue();
    }

    [Fact]
    public async Task Send_WithPreProcessor_ExecutesPreProcessor()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        services.AddTransient<IRequestPreProcessor<TestRequest>, TestPreProcessor>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new TestRequest { Value = "test" };

        TestPreProcessor.Called = false;

        // Act
        await mediator.Send(request);

        // Assert
        TestPreProcessor.Called.Should().BeTrue();
    }

    [Fact]
    public async Task Send_WithPostProcessor_ExecutesPostProcessor()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddTransient<IRequestHandler<TestRequest, string>, TestRequestHandler>();
        services.AddTransient<IRequestPostProcessor<TestRequest, string>, TestPostProcessor>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new TestRequest { Value = "test" };

        TestPostProcessor.Called = false;

        // Act
        await mediator.Send(request);

        // Assert
        TestPostProcessor.Called.Should().BeTrue();
    }

    // Test types
    public class TestRequest : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    public class TestRequestHandler : IRequestHandler<TestRequest, string>
    {
        public ValueTask<string> Handle(TestRequest request, CancellationToken cancellationToken)
        {
            return new ValueTask<string>($"Handled: {request.Value}");
        }
    }

    public class CancellableRequest : IRequest<string>
    {
    }

    public class CancellableRequestHandler : IRequestHandler<CancellableRequest, string>
    {
        public async ValueTask<string> Handle(CancellableRequest request, CancellationToken cancellationToken)
        {
            await Task.Delay(1000, cancellationToken);
            return "Done";
        }
    }

    public class TestPipelineBehavior : IPipelineBehavior<TestRequest, string>
    {
        public async ValueTask<string> Handle(
            TestRequest request,
            RequestHandlerDelegate<string> next,
            CancellationToken cancellationToken)
        {
            var response = await next();
            return $"Before -> {response} -> After";
        }
    }

    public class TestNotification : INotification
    {
    }

    public class TestNotificationHandler1 : INotificationHandler<TestNotification>
    {
        public static bool Called { get; set; }

        public ValueTask Handle(TestNotification notification, CancellationToken cancellationToken)
        {
            Called = true;
            return ValueTask.CompletedTask;
        }
    }

    public class TestNotificationHandler2 : INotificationHandler<TestNotification>
    {
        public static bool Called { get; set; }

        public ValueTask Handle(TestNotification notification, CancellationToken cancellationToken)
        {
            Called = true;
            return ValueTask.CompletedTask;
        }
    }

    public class TestPreProcessor : IRequestPreProcessor<TestRequest>
    {
        public static bool Called { get; set; }

        public ValueTask Process(TestRequest request, CancellationToken cancellationToken)
        {
            Called = true;
            return ValueTask.CompletedTask;
        }
    }

    public class TestPostProcessor : IRequestPostProcessor<TestRequest, string>
    {
        public static bool Called { get; set; }

        public ValueTask Process(TestRequest request, string response, CancellationToken cancellationToken)
        {
            Called = true;
            return ValueTask.CompletedTask;
        }
    }
}
