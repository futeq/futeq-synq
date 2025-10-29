using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ.Filters.UnitTests;

public class AuthorizationFilterTests
{
    [Fact]
    public async Task Allows_When_Guard_Passes()
    {
        var services = new ServiceCollection();
        
        services.AddLogging();
        services.AddTransient<IGuard<ActAdd>, AllowAllGuard<ActAdd>>();
        services.AddSynq(b => b
            .ScanHandlers(typeof(ActAddHandler).Assembly)
            .Pipelines(p => p.ForActs().UseAuthorization())
        );

        using var sp = services.BuildServiceProvider();
        var synq = sp.GetRequiredService<ISynq>();

        ActAddHandler.Reset();
        
        var result = await synq.Dispatch(new ActAdd(5));
        
        result.Should().Be(5);
    }

    [Fact]
    public async Task Denies_When_Guard_Throws()
    {
        var services = new ServiceCollection();
        
        services.AddLogging();
        services.AddTransient<IGuard<ActAdd>, DenyAllGuard<ActAdd>>();

        services.AddSynq(b => b
            .ScanHandlers(typeof(ActAddHandler).Assembly)
            .Pipelines(p => p.ForActs().UseAuthorization())
        );

        using var sp = services.BuildServiceProvider();
        var synq = sp.GetRequiredService<ISynq>();

        Func<Task> act = async () => _ = await synq.Dispatch(new ActAdd(1));
        
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("nope");
    }
}