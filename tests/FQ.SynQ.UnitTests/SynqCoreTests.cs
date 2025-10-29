using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ.UnitTests;

public class SynqCoreTests
{
    private static ServiceProvider BuildProvider(Action<SynqBuilder> configure)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSynq(configure);
        return services.BuildServiceProvider(validateScopes: true);
    }

    [Fact]
    public async Task Dispatch_Ask_RoutesToHandlerAndReturnsValue()
    {
        using var sp = BuildProvider(b =>
            b.ScanHandlers(typeof(AskEchoHandler).Assembly));

        var synq = sp.GetRequiredService<ISynq>();
        var result = await synq.Dispatch(new AskEcho("hi"));

        result.Should().Be("ECHO:hi");
    }

    [Fact]
    public async Task Dispatch_Act_RoutesToHandlerAndReturnsValue()
    {
        ActIncrementHandler.Reset();

        using var sp = BuildProvider(b =>
            b.ScanHandlers(typeof(ActIncrementHandler).Assembly));

        var synq = sp.GetRequiredService<ISynq>();
        var result1 = await synq.Dispatch(new ActIncrement(2));
        var result2 = await synq.Dispatch(new ActIncrement(3));

        result1.Should().Be(2);
        result2.Should().Be(5);
    }

    [Fact]
    public async Task NotRegisteredHandler_Throws()
    {
        using var sp = BuildProvider(b => { /* no ScanHandlers */ });

        var synq = sp.GetRequiredService<ISynq>();
        Func<Task> act = async () => await synq.Dispatch(new AskEcho("x"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No handler registered*");
    }
}