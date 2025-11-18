using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using AxisCore.Mediator.DependencyInjection;
using Xunit;
using System.Reflection;

namespace AxisCore.Mediator.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddMediator_RegistersMediator()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator();
        var provider = services.BuildServiceProvider();

        // Assert
        var mediator = provider.GetService<IMediator>();
        mediator.Should().NotBeNull();
        mediator.Should().BeOfType<Mediator>();
    }

    [Fact]
    public void AddMediatorFromAssembly_RegistersHandlers()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddMediator(new[] { Assembly.GetExecutingAssembly() });
        var provider = services.BuildServiceProvider();

        // Assert
        var handler = provider.GetService<IRequestHandler<SampleRequest, string>>();
        handler.Should().NotBeNull();
    }

    [Fact]
    public void AddRequestHandler_RegistersHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        // Act
        services.AddRequestHandler<SampleRequest, string, SampleRequestHandler>();
        var provider = services.BuildServiceProvider();

        // Assert
        var handler = provider.GetService<IRequestHandler<SampleRequest, string>>();
        handler.Should().NotBeNull();
        handler.Should().BeOfType<SampleRequestHandler>();
    }

    [Fact]
    public void AddNotificationHandler_RegistersHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();

        // Act
        services.AddNotificationHandler<SampleNotification, SampleNotificationHandler>();
        var provider = services.BuildServiceProvider();

        // Assert
        var handler = provider.GetService<INotificationHandler<SampleNotification>>();
        handler.Should().NotBeNull();
        handler.Should().BeOfType<SampleNotificationHandler>();
    }

    [Fact]
    public async Task RegisteredHandlers_WorkCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator(new[] { Assembly.GetExecutingAssembly() });
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();

        // Act
        var result = await mediator.Send(new SampleRequest { Value = "test" });

        // Assert
        result.Should().Be("Sample: test");
    }

    // Test types
    public class SampleRequest : IRequest<string>
    {
        public string Value { get; set; } = string.Empty;
    }

    public class SampleRequestHandler : IRequestHandler<SampleRequest, string>
    {
        public ValueTask<string> Handle(SampleRequest request, CancellationToken cancellationToken)
        {
            return new ValueTask<string>($"Sample: {request.Value}");
        }
    }

    public class SampleNotification : INotification
    {
    }

    public class SampleNotificationHandler : INotificationHandler<SampleNotification>
    {
        public ValueTask Handle(SampleNotification notification, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }
    }
}
