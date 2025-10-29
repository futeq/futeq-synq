using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FQ.SynQ.Filters.UnitTests;

public class PerformanceFilterTests
{
    [Fact]
    public async Task Runs_And_Does_Not_Alter_Result()
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddDebug());

        services.AddSynq(b => b
            .ScanHandlers(typeof(AskPingHandler).Assembly)
            .Pipelines(p => p.UsePerformance())
        );

        await using var sp = services.BuildServiceProvider();
        var synq = sp.GetRequiredService<ISynq>();

        var result = await synq.Dispatch(new AskPing("perf"));
        result.Should().Be("pong:perf");
    }
}