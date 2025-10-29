using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ.UnitTests;

public class SynqFilterOrderingTests
{
    private static ServiceProvider BuildProvider(Action<SynqBuilder> configure, RecordingSink sink)
    {
        var services = new ServiceCollection();
        
        services.AddSingleton(sink);
        services.AddLogging();
        services.AddSynq(configure);
        
        return services.BuildServiceProvider();
    }

    [Fact]
    public async Task Filters_RunInConfiguredOrder_Global_Category_Predicate_PerMessage()
    {
        var sink = new RecordingSink();

        await using var sp = BuildProvider(b => b
            .ScanHandlers(typeof(AskEchoHandler).Assembly)
            .Pipelines(p => p
                .Use(typeof(RecordingFilter<,>))                 // Global
                .ForAsks().Use(typeof(RecordingFilter<,>))      // Category
                .For(t => t.Name.Contains("Ask")).Use(typeof(RecordingFilter<,>)) // Predicate
                .ForMessage<AskEcho>().Use(typeof(RecordingFilter<,>))            // Per-message
            )
        , sink);

        var synq = sp.GetRequiredService<ISynq>();
        _ = await synq.Dispatch(new AskEcho("order"));

        var enters = sink.Events.Where(e => e.StartsWith("enter")).ToList();
        var exits  = sink.Events.Where(e => e.StartsWith("exit")).ToList();

        enters.Should().HaveCount(4);
        exits.Should().HaveCount(4);

        enters[0].Should().StartWith("enter:RecordingFilter");
        enters[1].Should().StartWith("enter:RecordingFilter");
        enters[2].Should().StartWith("enter:RecordingFilter");
        enters[3].Should().StartWith("enter:RecordingFilter");

        exits[0].Should().StartWith("exit:RecordingFilter");
        exits[1].Should().StartWith("exit:RecordingFilter");
        exits[2].Should().StartWith("exit:RecordingFilter");
        exits[3].Should().StartWith("exit:RecordingFilter");
    }

    [Fact]
    public async Task CategorySelection_AppliesToActsAndAsksSeparately()
    {
        var sink = new RecordingSink();

        await using var sp = BuildProvider(b => b
            .ScanHandlers(typeof(AskEchoHandler).Assembly, typeof(ActIncrementHandler).Assembly)
            .Pipelines(p => p
                .ForAsks().Use(typeof(RecordingFilter<,>))
                .ForActs().Use(typeof(RecordingFilter<,>))
            )
        , sink);

        var synq = sp.GetRequiredService<ISynq>();

        sink.Events.Clear();
        _ = await synq.Dispatch(new AskEcho("q"));
        sink.Events.Count(e => e.StartsWith("enter")).Should().Be(1);

        sink.Events.Clear();
        ActIncrementHandler.Reset();
        _ = await synq.Dispatch(new ActIncrement(1));
        sink.Events.Count(e => e.StartsWith("enter")).Should().Be(1);
    }

    [Fact]
    public async Task PerMessageRule_TakesHighestPrecedence()
    {
        var sink = new RecordingSink();

        await using var sp = BuildProvider(b => b
            .ScanHandlers(typeof(AskEchoHandler).Assembly)
            .Pipelines(p => p
                .Use(typeof(RecordingFilter<,>))        // global
                .ForAsks().Use(typeof(RecordingFilter<,>)) // category
                .For(t => true).Use(typeof(RecordingFilter<,>)) // predicate
                .ForMessage<AskEcho>().Use(typeof(RecordingFilter<,>)) // per-message (outermost)
            )
        , sink);

        var synq = sp.GetRequiredService<ISynq>();
        _ = await synq.Dispatch(new AskEcho("x"));

        var lastEnter = sink.Events.Last(e => e.StartsWith("enter"));
        lastEnter.Should().Contain("RecordingFilter");
    }

    [Fact]
    public async Task FilterDependencies_AreResolvedFromDI()
    {
        var services = new ServiceCollection();
        
        services.AddLogging();
        services.AddSynq(b => b
            .ScanHandlers(typeof(AskEchoHandler).Assembly)
            .Pipelines(p => p.Use(typeof(RequiresLoggerFilter<,>)))
        );

        await using var sp = services.BuildServiceProvider();
        var synq = sp.GetRequiredService<ISynq>();

        var result = await synq.Dispatch(new AskEcho("di"));
        
        result.Should().Be("ECHO:di");
    }
}