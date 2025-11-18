using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace AxisCore.Mediator.Behaviors;

/// <summary>
/// Pipeline behavior that measures and logs request execution time.
/// </summary>
/// <typeparam name="TRequest">Request type</typeparam>
/// <typeparam name="TResponse">Response type</typeparam>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly TimeSpan _warningThreshold;

    /// <summary>
    /// Initializes a new instance of the <see cref="PerformanceBehavior{TRequest, TResponse}"/> class.
    /// </summary>
    /// <param name="logger">Logger instance</param>
    /// <param name="warningThreshold">Threshold for logging slow requests (default: 500ms)</param>
    public PerformanceBehavior(
        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
        TimeSpan? warningThreshold = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _warningThreshold = warningThreshold ?? TimeSpan.FromMilliseconds(500);
    }

    /// <inheritdoc />
    public async ValueTask<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            return await next().ConfigureAwait(false);
        }
        finally
        {
            stopwatch.Stop();

            if (stopwatch.Elapsed > _warningThreshold)
            {
                _logger.LogWarning(
                    "Long running request {RequestName} took {ElapsedMs}ms (threshold: {ThresholdMs}ms)",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    _warningThreshold.TotalMilliseconds);
            }
            else
            {
                _logger.LogDebug(
                    "Request {RequestName} completed in {ElapsedMs}ms",
                    requestName,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
