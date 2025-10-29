namespace FQ.SynQ;

/// <summary>Pipeline filter around message handling.</summary>
public interface IFilter<T, TOut>
    where T : IMessage<TOut>
{
    Task<TOut> Invoke(T message, CancellationToken ct, Next<TOut> next);
}