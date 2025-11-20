using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace AxisCore.Mediator.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.NativeAot90)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net60)]
public class RequestBenchmarks
{
    private IServiceProvider _axiscoreProvider = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Setup AxisCore.Mediator
        var axiscoreServices = new ServiceCollection();
        axiscoreServices.AddMediator();
        axiscoreServices.AddTransient<IRequestHandler<AxisCorePingRequest, string>, AxisCorePingHandler>();
        _axiscoreProvider = axiscoreServices.BuildServiceProvider();
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
}
