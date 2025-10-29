using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ.Filters.UnitTests;

public class UnitOfWorkFilterTests
{
    [Fact]
    public async Task Commit_On_Success()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<FakeUnitOfWork>();
        services.AddSingleton<IUnitOfWork>(sp => sp.GetRequiredService<FakeUnitOfWork>());

        services.AddSynq(b => b
            .ScanHandlers(typeof(ActAddHandler).Assembly)
            .Pipelines(p => p.ForActs().UseUnitOfWork())
        );

        await using var sp = services.BuildServiceProvider();
        var uow = sp.GetRequiredService<FakeUnitOfWork>();
        var synq = sp.GetRequiredService<ISynq>();

        ActAddHandler.Reset();
        var result = await synq.Dispatch(new ActAdd(7));

        result.Should().Be(7);
        uow.Calls.Should().ContainInOrder("begin", "commit");
        uow.Calls.Should().NotContain("rollback");
    }

    [Fact]
    public async Task Rollback_On_Failure()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<FakeUnitOfWork>();
        services.AddSingleton<IUnitOfWork>(sp => sp.GetRequiredService<FakeUnitOfWork>());

        services.AddSynq(b => b
            .ScanHandlers(typeof(ActBoomHandler).Assembly)
            .Pipelines(p => p.ForActs().UseUnitOfWork())
        );

        await using var sp = services.BuildServiceProvider();
        var uow = sp.GetRequiredService<FakeUnitOfWork>();
        var synq = sp.GetRequiredService<ISynq>();

        var act = async () => await synq.Dispatch(new ActBoom());
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("kaboom");

        uow.Calls.Should().ContainInOrder("begin", "rollback");
        uow.Calls.Should().NotContain("commit");
    }
}