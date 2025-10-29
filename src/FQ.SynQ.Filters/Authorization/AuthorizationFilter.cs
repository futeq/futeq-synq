namespace FQ.SynQ.Filters.Authorization; 

/// <summary>Executes all registered <see cref="IGuard{T}"/> for the message; throw inside guards to deny.</summary>
public sealed class AuthorizationFilter<T, TOut> : IFilter<T, TOut>
    where T : IMessage<TOut>
{
    private readonly IEnumerable<IGuard<T>> _guards;

    public AuthorizationFilter(IEnumerable<IGuard<T>> guards)
    {
        _guards = guards ?? [];
    }

    public async Task<TOut> Invoke(T message, CancellationToken ct, Next<TOut> next)
    {
        foreach (var g in _guards)
        {
            await g.CheckAsync(message, ct);
        }
        
        return await next(ct);
    }
}