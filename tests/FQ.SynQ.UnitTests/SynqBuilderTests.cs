using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ.UnitTests;

public class SynqBuilderTests
{
    [Fact]
    public async Task ScanHandlers_RegistersAllHandlersFromAssembly()
    {
        var services = new ServiceCollection();
        services.AddSynq(b =>
            b.ScanHandlers(typeof(AskEchoHandler).Assembly)
        );

        await using var sp = services.BuildServiceProvider();
        var synq = sp.GetRequiredService<ISynq>();
        var res = await synq.Dispatch(new AskEcho("bulk"));

        res.Should().Be("ECHO:bulk");
    }

    [Fact]
    public void Pipelines_NoConfig_BuildsSuccessfully()
    {
        var services = new ServiceCollection();
        Action act = () => services.AddSynq(b => b.ScanHandlers(typeof(AskEchoHandler).Assembly))
            .BuildServiceProvider();

        act.Should().NotThrow();
    }
}