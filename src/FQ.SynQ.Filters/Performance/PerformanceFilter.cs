using Microsoft.Extensions.Logging;

namespace FQ.SynQ.Filters.Performance;

/// <summary>Measures handler execution time and logs when provided with an <see cref="ILogger"/>.</summary>
public sealed class PerformanceFilter<T, TOut> : IFilter<T, TOut>
    where T : IMessage<TOut>
{
    private readonly ILogger<PerformanceFilter<T, TOut>>? _logger;

    public PerformanceFilter(ILogger<PerformanceFilter<T, TOut>>? logger = null)
    {
        _logger = logger;
    }

    public async Task<TOut> Invoke(T message, CancellationToken ct, Next<TOut> next)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            return await next(ct);
        }
        finally
        {
            sw.Stop();
            
            _logger?.LogDebug("SynQ {Message} handled in {Elapsed} ms", typeof(T).Name, sw.Elapsed.TotalMilliseconds);
        }
    }
}