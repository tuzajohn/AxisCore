using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace AxisCore.Mediator.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net60)]
public class NotificationBenchmarks
{
    private IServiceProvider _axiscoreProvider = null!;
    private IServiceProvider _mediatrProvider = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Setup AxisCore.Mediator
        var axiscoreServices = new ServiceCollection();
        axiscoreServices.AddMediator();
        axiscoreServices.AddTransient<AxisCore.Mediator.INotificationHandler<AxisCorePingNotification>, AxisCorePingNotificationHandler1>();
        axiscoreServices.AddTransient<AxisCore.Mediator.INotificationHandler<AxisCorePingNotification>, AxisCorePingNotificationHandler2>();
        axiscoreServices.AddTransient<AxisCore.Mediator.INotificationHandler<AxisCorePingNotification>, AxisCorePingNotificationHandler3>();
        _axiscoreProvider = axiscoreServices.BuildServiceProvider();

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
    public async Task AxisCore_Publish()
    {
        var mediator = _axiscoreProvider.GetRequiredService<AxisCore.Mediator.IMediator>();
        await mediator.Publish(new AxisCorePingNotification());
    }

    // AxisCore types
    public class AxisCorePingNotification : AxisCore.Mediator.INotification
    {
    }

    public class AxisCorePingNotificationHandler1 : AxisCore.Mediator.INotificationHandler<AxisCorePingNotification>
    {
        public ValueTask Handle(AxisCorePingNotification notification, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }
    }

    public class AxisCorePingNotificationHandler2 : AxisCore.Mediator.INotificationHandler<AxisCorePingNotification>
    {
        public ValueTask Handle(AxisCorePingNotification notification, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }
    }

    public class AxisCorePingNotificationHandler3 : AxisCore.Mediator.INotificationHandler<AxisCorePingNotification>
    {
        public ValueTask Handle(AxisCorePingNotification notification, CancellationToken cancellationToken)
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
