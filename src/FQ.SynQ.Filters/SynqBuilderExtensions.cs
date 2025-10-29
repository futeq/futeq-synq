namespace FQ.SynQ.Filters;

/// <summary>
/// Convenience presets for composing SynQ pipelines.
/// Keeps filter classes internal while exposing a small, stable API.
/// </summary>
public static class SynqBuilderExtensions
{
    /// <summary>
    /// Adds a sensible default set of filters for typical apps:
    /// - Global Performance
    /// - Validation for queries
    /// - Authorization for commands
    /// </summary>
    public static SynqBuilder AddCommonFilters(
        this SynqBuilder builder,
        bool includeValidation = true,
        bool includeAuthorization = true,
        bool includePerformance = true)
    {
        return builder.Pipelines(p =>
        {
            if (includePerformance) p.UsePerformance(); // global
            if (includeValidation) p.ForAsks().UseValidation();
            if (includeAuthorization) p.ForActs().UseAuthorization();
        });
    }

    /// <summary>
    /// Adds typical write-side (act) filters:
    /// - Idempotency
    /// - Unit of Work
    /// - Domain Events
    /// </summary>
    public static SynqBuilder AddCommonActFilters(
        this SynqBuilder builder,
        bool includeIdempotency = true,
        bool includeUnitOfWork = true,
        bool includeDomainEvents = true)
    {
        return builder.Pipelines(p =>
        {
            if (includeIdempotency) p.ForActs().UseIdempotency();
            if (includeUnitOfWork) p.ForActs().UseUnitOfWork();
            if (includeDomainEvents) p.ForActs().UseDomainEvents();
        });
    }

    /// <summary>
    /// A strict preset for read/write separation:
    /// - Validation for both commands and queries
    /// - Authorization only for commands
    /// - UoW + Domain Events for commands
    /// - Global Performance
    /// </summary>
    public static SynqBuilder AddStrictReadWriteDefaults(this SynqBuilder builder)
    {
        return builder.Pipelines(p =>
        {
            p.UsePerformance(); // global
            p.ForAsks().UseValidation(); // queries
            p.ForActs().UseValidation() // commands
                .UseAuthorization()
                .UseUnitOfWork()
                .UseDomainEvents();
        });
    }

    /// <summary>
    /// A compact web-friendly preset:
    /// - Global Performance
    /// - Validation (queries + commands)
    /// - Authorization (commands)
    /// - Optional: Idempotency (commands)
    /// </summary>
    public static SynqBuilder AddWebApiDefaults(
        this SynqBuilder builder,
        bool includeIdempotency = true)
    {
        return builder.Pipelines(p =>
        {
            p.UsePerformance(); // global
            p.ForAsks().UseValidation(); // queries
            p.ForActs().UseValidation()
                .UseAuthorization();

            if (includeIdempotency)
                p.ForActs().UseIdempotency();
        });
    }

    /// <summary>
    /// Apply a preset only to message types matching the predicate.
    /// Example: restrict extra auth to Admin namespace.
    /// </summary>
    public static SynqBuilder AddPredicatePreset(
        this SynqBuilder builder,
        Func<Type, bool> predicate,
        bool auth = false,
        bool validation = false,
        bool perf = false,
        bool uow = false,
        bool domainEvents = false,
        bool idempotency = false)
    {
        return builder.Pipelines(p =>
        {
            var pc = p.For(predicate);
            if (perf) pc.UsePerformance();
            if (validation) pc.UseValidation();
            if (auth) pc.UseAuthorization();
            if (idempotency) pc.UseIdempotency();
            if (uow) pc.UseUnitOfWork();
            if (domainEvents) pc.UseDomainEvents();
        });
    }
}