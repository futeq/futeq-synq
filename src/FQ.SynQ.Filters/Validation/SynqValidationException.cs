namespace FQ.SynQ.Filters.Validation;

/// <summary>Thrown when validation fails in <see cref="ValidationFilter{T,TOut}"/>.</summary>
public sealed class SynqValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }
    public SynqValidationException(IReadOnlyDictionary<string, string[]> errors, string? message = null)
        : base(message ?? "Validation failed.") => Errors = errors;
}