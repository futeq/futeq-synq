using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ.Filters.UnitTests;

public class IdempotencyFilterTests
{
    [Fact]
    public async Task Without_Key_Acts_Like_NoOp()
    {
        ActAddHandler.Reset();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IIdempotencyKeyAccessor>(new FixedIdempotencyKeyAccessor(null));
        services.AddSingleton<IIdempotencyStore, MemoryIdempotencyStore>();

        services.AddSynq(b => b
            .ScanHandlers(typeof(ActAddHandler).Assembly)
            .Pipelines(p => p.ForActs().UseIdempotency())
        );

        using var sp = services.BuildServiceProvider();
        var synq = sp.GetRequiredService<ISynq>();

        var r1 = await synq.Dispatch(new ActAdd(3));
        var r2 = await synq.Dispatch(new ActAdd(4));

        r1.Should().Be(3);
        r2.Should().Be(7);
    }

    [Fact]
    public async Task With_Key_Caches_First_Result_And_Replays_It()
    {
        ActAddHandler.Reset();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IIdempotencyKeyAccessor>(new FixedIdempotencyKeyAccessor("K-123"));
        services.AddSingleton<IIdempotencyStore, MemoryIdempotencyStore>();

        services.AddSynq(b => b
            .ScanHandlers(typeof(ActAddHandler).Assembly)
            .Pipelines(p => p.ForActs().UseIdempotency())
        );

        using var sp = services.BuildServiceProvider();
        var synq = sp.GetRequiredService<ISynq>();

        var first = await synq.Dispatch(new ActAdd(5));
        var replay = await synq.Dispatch(new ActAdd(99)); // would normally add 99, but should be replayed

        first.Should().Be(5);
        replay.Should().Be(5);
    }
}