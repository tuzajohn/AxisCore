using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace AxisCore.Mediator.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net100)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net60)]
public class RequestBenchmarks
{
    private IServiceProvider _axiscoreProvider = null!;
    private IServiceProvider _mediatrProvider = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Setup AxisCore.Mediator
        var axiscoreServices = new ServiceCollection();
        axiscoreServices.AddMediator();
        axiscoreServices.AddTransient<IRequestHandler<AxisCorePingRequest, string>, AxisCorePingHandler>();
        _axiscoreProvider = axiscoreServices.BuildServiceProvider();

        // Setup MediatR
        var mediatrServices = new ServiceCollection();
        mediatrServices.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RequestBenchmarks).Assembly));
        _mediatrProvider = mediatrServices.BuildServiceProvider();
    }

    [Benchmark(Baseline = true)]
    public async Task<string> MediatR_Send()
    {
        var mediator = _mediatrProvider.GetRequiredService<MediatR.IMediator>();
        return await mediator.Send(new MediatrPingRequest());
    }

    [Benchmark]
    public async Task<string> AxisCore_Send()
    {
        var mediator = _axiscoreProvider.GetRequiredService<AxisCore.Mediator.IMediator>();
        return await mediator.Send(new AxisCorePingRequest());
    }

    // AxisCore types
    public class AxisCorePingRequest : AxisCore.Mediator.IRequest<string>
    {
    }

    public class AxisCorePingHandler : AxisCore.Mediator.IRequestHandler<AxisCorePingRequest, string>
    {
        public ValueTask<string> Handle(AxisCorePingRequest request, CancellationToken cancellationToken)
        {
            return new ValueTask<string>("Pong");
        }
    }

    // MediatR types
    public class MediatrPingRequest : MediatR.IRequest<string>
    {
    }

    public class MediatrPingHandler : MediatR.IRequestHandler<MediatrPingRequest, string>
    {
        public Task<string> Handle(MediatrPingRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult("Pong");
        }
    }
}
