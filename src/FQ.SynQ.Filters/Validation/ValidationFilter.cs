using FluentValidation;
using FluentValidation.Results;

namespace FQ.SynQ.Filters.Validation;

/// <summary>Runs FluentValidation validators for the message and throws on failures.</summary>
internal sealed class ValidationFilter<T, TOut> : IFilter<T, TOut>
    where T : IMessage<TOut>
{
    private readonly IEnumerable<IValidator<T>> _validators;

    public ValidationFilter(IEnumerable<IValidator<T>>? validators)
        => _validators = validators ?? [];

    public async Task<TOut> Invoke(T message, CancellationToken ct, Next<TOut> next)
    {
        if (_validators.Any())
        {
            var ctx = new ValidationContext<T>(message);
            var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(ctx, ct)));
            var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToArray();

            if (failures.Length > 0)
                throw new SynqValidationException(ToMap(failures));
        }

        return await next(ct);
    }

    private static IReadOnlyDictionary<string, string[]> ToMap(IEnumerable<ValidationFailure> fails) =>
        fails.GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray());
}