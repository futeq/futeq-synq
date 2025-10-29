using FQ.SynQ.Configuration;
using FQ.SynQ.Filters.Authorization;
using FQ.SynQ.Filters.DomainEvents;
using FQ.SynQ.Filters.Idempotency;
using FQ.SynQ.Filters.Performance;
using FQ.SynQ.Filters.UnitOfWork;
using FQ.SynQ.Filters.Validation;

namespace FQ.SynQ.Filters;

public static partial class ConfigurationExtensions
{
    public static PipelineConfigurator UsePerformance(this PipelineConfigurator p)
        => p.Use(typeof(PerformanceFilter<,>));

    public static PipelineConfigurator UseValidation(this PipelineConfigurator p)
        => p.Use(typeof(ValidationFilter<,>));

    public static PipelineConfigurator UseAuthorization(this PipelineConfigurator p)
        => p.Use(typeof(AuthorizationFilter<,>));

    public static PipelineConfigurator UseIdempotency(this PipelineConfigurator p)
        => p.Use(typeof(IdempotencyFilter<,>));

    public static PipelineConfigurator UseUnitOfWork(this PipelineConfigurator p)
        => p.Use(typeof(UnitOfWorkFilter<,>));

    public static PipelineConfigurator UseDomainEvents(this PipelineConfigurator p)
        => p.Use(typeof(DomainEventsFilter<,>));
}