using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ.Filters.UnitTests;

public class DomainEventsFilterTests
{
    [Fact]
    public async Task Publishes_After_Successful_Command()
    {
        var services = new ServiceCollection();
        
        services.AddLogging();
        services.AddSingleton<FakeDomainEventsPublisher>();
        services.AddSingleton<IDomainEventsPublisher>(sp => sp.GetRequiredService<FakeDomainEventsPublisher>());

        services.AddSynq(b => b
            .ScanHandlers(typeof(ActAddHandler).Assembly)
            .Pipelines(p => p.ForActs().UseDomainEvents())
        );

        await using var sp = services.BuildServiceProvider();
        var pub = sp.GetRequiredService<FakeDomainEventsPublisher>();
        var synq = sp.GetRequiredService<ISynq>();

        ActAddHandler.Reset();
        
        var res = await synq.Dispatch(new ActAdd(2));

        res.Should().Be(2);
        pub.Published.Should().Be(1);
    }
}