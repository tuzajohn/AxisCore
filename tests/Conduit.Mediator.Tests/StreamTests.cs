using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Conduit.Mediator.DependencyInjection;
using Xunit;

namespace Conduit.Mediator.Tests;

public class StreamTests
{
    [Fact]
    public async Task CreateStream_ReturnsAsyncEnumerable()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddTransient<IStreamRequestHandler<StreamRequest, int>, StreamRequestHandler>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new StreamRequest { Count = 5 };

        // Act
        var results = new List<int>();
        await foreach (var item in mediator.CreateStream(request))
        {
            results.Add(item);
        }

        // Assert
        results.Should().Equal(0, 1, 2, 3, 4);
    }

    [Fact]
    public async Task CreateStream_WithCancellation_StopsStream()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediator();
        services.AddTransient<IStreamRequestHandler<StreamRequest, int>, StreamRequestHandler>();
        var provider = services.BuildServiceProvider();
        var mediator = provider.GetRequiredService<IMediator>();
        var request = new StreamRequest { Count = 100 };
        var cts = new CancellationTokenSource();

        // Act
        var results = new List<int>();
        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in mediator.CreateStream(request, cts.Token))
            {
                results.Add(item);
                if (results.Count == 5)
                {
                    cts.Cancel();
                }
            }
        });

        // Assert
        results.Count.Should().BeLessThan(100);
    }

    // Test types
    public class StreamRequest : IStreamRequest<int>
    {
        public int Count { get; set; }
    }

    public class StreamRequestHandler : IStreamRequestHandler<StreamRequest, int>
    {
        public async IAsyncEnumerable<int> Handle(
            StreamRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (int i = 0; i < request.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(10, cancellationToken);
                yield return i;
            }
        }
    }
}
