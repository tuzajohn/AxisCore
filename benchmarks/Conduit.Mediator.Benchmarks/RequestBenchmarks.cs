using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace Conduit.Mediator.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net80)]
public class RequestBenchmarks
{
    private IServiceProvider _conduitProvider = null!;
    private IServiceProvider _mediatrProvider = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Setup Conduit.Mediator
        var conduitServices = new ServiceCollection();
        conduitServices.AddMediator();
        conduitServices.AddTransient<IRequestHandler<ConduitPingRequest, string>, ConduitPingHandler>();
        _conduitProvider = conduitServices.BuildServiceProvider();

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
    public async Task<string> Conduit_Send()
    {
        var mediator = _conduitProvider.GetRequiredService<Conduit.Mediator.IMediator>();
        return await mediator.Send(new ConduitPingRequest());
    }

    // Conduit types
    public class ConduitPingRequest : Conduit.Mediator.IRequest<string>
    {
    }

    public class ConduitPingHandler : Conduit.Mediator.IRequestHandler<ConduitPingRequest, string>
    {
        public ValueTask<string> Handle(ConduitPingRequest request, CancellationToken cancellationToken)
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
