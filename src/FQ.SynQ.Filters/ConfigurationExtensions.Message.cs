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
    public static PipelineConfigurator UsePerformance(this MessageConfigurator m)
        => m.Use(typeof(PerformanceFilter<,>));

    public static PipelineConfigurator UseValidation(this MessageConfigurator m)
        => m.Use(typeof(ValidationFilter<,>));

    public static PipelineConfigurator UseAuthorization(this MessageConfigurator m)
        => m.Use(typeof(AuthorizationFilter<,>));

    public static PipelineConfigurator UseIdempotency(this MessageConfigurator m)
        => m.Use(typeof(IdempotencyFilter<,>));

    public static PipelineConfigurator UseUnitOfWork(this MessageConfigurator m)
        => m.Use(typeof(UnitOfWorkFilter<,>));

    public static PipelineConfigurator UseDomainEvents(this MessageConfigurator m)
        => m.Use(typeof(DomainEventsFilter<,>));
}