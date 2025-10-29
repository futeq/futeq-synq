using Microsoft.Extensions.Logging;

namespace FQ.SynQ.UnitTests;

public sealed record AskEcho(string Text) : IAsk<string>;
public sealed record ActIncrement(int By) : IAct<int>;

public sealed class AskEchoHandler : IMessageHandler<AskEcho, string>
{
    public Task<string> Handle(AskEcho message, CancellationToken ct) => Task.FromResult($"ECHO:{message.Text}");
}

public sealed class ActIncrementHandler : IMessageHandler<ActIncrement, int>
{
    private static int _value;
    public Task<int> Handle(ActIncrement message, CancellationToken ct)
    {
        _value += message.By;
        return Task.FromResult(_value);
    }

    public static void Reset() => _value = 0;
}

/// <summary>
/// A test filter that records enter/exit markers to a shared list.
/// </summary>
public sealed class RecordingFilter<T, TOut> : IFilter<T, TOut>
    where T : IMessage<TOut>
{
    private readonly RecordingSink _sink;
    private readonly string _name;

    public RecordingFilter(RecordingSink sink)
    {
        _sink = sink;
        _name = $"{GetType().Name}[{typeof(T).Name}->{typeof(TOut).Name}]";
    }

    public async Task<TOut> Invoke(T message, CancellationToken ct, Next<TOut> next)
    {
        _sink.Events.Add($"enter:{_name}");
        try
        {
            var result = await next(ct);
            _sink.Events.Add($"exit:{_name}");
            return result;
        }
        catch
        {
            _sink.Events.Add($"throw:{_name}");
            throw;
        }
    }
}

public sealed class RecordingSink
{
    public List<string> Events { get; } = new();
}

/// <summary>
/// Filter that requires DI dependency (ILogger) to verify ActivatorUtilities injection.
/// </summary>
public sealed class RequiresLoggerFilter<T, TOut> : IFilter<T, TOut>
    where T : IMessage<TOut>
{
    private readonly ILogger<RequiresLoggerFilter<T, TOut>> _logger;
    public RequiresLoggerFilter(ILogger<RequiresLoggerFilter<T, TOut>> logger) => _logger = logger;

    public Task<TOut> Invoke(T message, CancellationToken ct, Next<TOut> next)
        => next(ct);
}