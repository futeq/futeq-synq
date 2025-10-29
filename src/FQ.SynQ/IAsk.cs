namespace FQ.SynQ;

/// <summary>
/// Marker for read-only operations that return data and must not change state.
/// </summary>
/// <typeparam name="TOut">The value returned on success.</typeparam>
public interface IAsk<TOut> : IMessage<TOut> { }