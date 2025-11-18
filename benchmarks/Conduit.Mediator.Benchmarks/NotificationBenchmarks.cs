using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace Conduit.Mediator.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net80)]
public class NotificationBenchmarks
{
    private IServiceProvider _conduitProvider = null!;
    private IServiceProvider _mediatrProvider = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Setup Conduit.Mediator
        var conduitServices = new ServiceCollection();
        conduitServices.AddMediator();
        conduitServices.AddTransient<Conduit.Mediator.INotificationHandler<ConduitPingNotification>, ConduitPingNotificationHandler1>();
        conduitServices.AddTransient<Conduit.Mediator.INotificationHandler<ConduitPingNotification>, ConduitPingNotificationHandler2>();
        conduitServices.AddTransient<Conduit.Mediator.INotificationHandler<ConduitPingNotification>, ConduitPingNotificationHandler3>();
        _conduitProvider = conduitServices.BuildServiceProvider();

        // Setup MediatR
        var mediatrServices = new ServiceCollection();
        mediatrServices.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(NotificationBenchmarks).Assembly));
        _mediatrProvider = mediatrServices.BuildServiceProvider();
    }

    [Benchmark(Baseline = true)]
    public async Task MediatR_Publish()
    {
        var mediator = _mediatrProvider.GetRequiredService<MediatR.IMediator>();
        await mediator.Publish(new MediatrPingNotification());
    }

    [Benchmark]
    public async Task Conduit_Publish()
    {
        var mediator = _conduitProvider.GetRequiredService<Conduit.Mediator.IMediator>();
        await mediator.Publish(new ConduitPingNotification());
    }

    // Conduit types
    public class ConduitPingNotification : Conduit.Mediator.INotification
    {
    }

    public class ConduitPingNotificationHandler1 : Conduit.Mediator.INotificationHandler<ConduitPingNotification>
    {
        public ValueTask Handle(ConduitPingNotification notification, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }
    }

    public class ConduitPingNotificationHandler2 : Conduit.Mediator.INotificationHandler<ConduitPingNotification>
    {
        public ValueTask Handle(ConduitPingNotification notification, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }
    }

    public class ConduitPingNotificationHandler3 : Conduit.Mediator.INotificationHandler<ConduitPingNotification>
    {
        public ValueTask Handle(ConduitPingNotification notification, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }
    }

    // MediatR types
    public class MediatrPingNotification : MediatR.INotification
    {
    }

    public class MediatrPingNotificationHandler1 : MediatR.INotificationHandler<MediatrPingNotification>
    {
        public Task Handle(MediatrPingNotification notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class MediatrPingNotificationHandler2 : MediatR.INotificationHandler<MediatrPingNotification>
    {
        public Task Handle(MediatrPingNotification notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class MediatrPingNotificationHandler3 : MediatR.INotificationHandler<MediatrPingNotification>
    {
        public Task Handle(MediatrPingNotification notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
