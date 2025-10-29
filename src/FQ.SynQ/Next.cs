namespace FQ.SynQ;

/// <summary>
/// Represents the next delegate in the pipeline for a given request,
/// returning a <typeparamref name="TResponse"/> when invoked.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <param name="ct">A cancellation token.</param>
public delegate Task<TResponse> Next<TResponse>(CancellationToken ct);