using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ.Dispatchers;

public sealed class SynqDispatcher : ISynq
{
    private readonly IServiceProvider _sp;
    private readonly IFilterCatalog _catalog;

    public SynqDispatcher(IServiceProvider sp, IFilterCatalog catalog)
    {
        _sp = sp;
        _catalog = catalog;
    }

    public Task<TOut> Dispatch<TOut>(IMessage<TOut> message, CancellationToken ct = default)
    {
        var msgType = message.GetType();
        var outType = typeof(TOut);

        var handlerType = typeof(IMessageHandler<,>).MakeGenericType(msgType, outType);
        var handler = _sp.GetService(handlerType)
            ?? throw new InvalidOperationException($"No handler registered for {msgType.FullName}.");

        var handle = handlerType.GetMethod(nameof(IMessageHandler<IMessage<TOut>, TOut>.Handle))!;

        Next<TOut> terminal = _ =>
        {
            try
            {
                return (Task<TOut>)handle.Invoke(handler, [message, ct])!;
            }
            catch (TargetInvocationException tie) when (tie.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
                
                throw;
            }
        };

        var filters = _catalog.CreateFilters(msgType, outType, _sp).ToArray();
        var next = terminal;
        
        foreach (var f in filters.Reverse())
        {
            var closedFilterType = typeof(IFilter<,>).MakeGenericType(msgType, outType);
            var invoke = closedFilterType.GetMethod(nameof(IFilter<IMessage<TOut>, TOut>.Invoke))!;
            var currentNext = next;

            next = token =>
            {
                try
                {
                    return (Task<TOut>)invoke.Invoke(f, [message, token, currentNext])!;
                }
                catch (TargetInvocationException tie) when (tie.InnerException is not null)
                {
                    ExceptionDispatchInfo.Capture(tie.InnerException).Throw();
                    throw; 
                }
            };
        }

        return next(ct);
    }
}