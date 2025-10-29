namespace FQ.SynQ;

/// <summary>
/// Marker for write operations that change system state and do not return a value.
/// Handlers should return <see cref="Nil"/>.
/// </summary>
public interface IAct : IMessage<Nil> { }

/// <summary>
/// Marker for write operations that change system state and return a value.
/// </summary>
/// <typeparam name="TOut">The value returned on success.</typeparam>
public interface IAct<TOut> : IMessage<TOut> { }