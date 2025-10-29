using FluentAssertions;
using FluentValidation;
using FQ.SynQ.Filters.Validation;
using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ.Filters.UnitTests;

public class ValidationFilterTests
{
    [Fact]
    public async Task Valid_Message_Passes_To_Handler()
    {
        var services = new ServiceCollection();
        
        services.AddLogging();
        services.AddSynq(b => b
            .ScanHandlers(typeof(AskPingHandler).Assembly)
            .Pipelines(p => p.ForAsks().UseValidation())
        );
        services.AddValidatorsFromAssemblyContaining<AskPingValidator>();

        await using var sp = services.BuildServiceProvider();
        var synq = sp.GetRequiredService<ISynq>();

        var result = await synq.Dispatch(new AskPing("ok"));
        result.Should().Be("pong:ok");
    }

    [Fact]
    public async Task Invalid_Message_Throws_SynqValidationException_With_FieldMap()
    {
        var services = new ServiceCollection();
        
        services.AddLogging();
        services.AddSynq(b => b
            .ScanHandlers(typeof(AskPingHandler).Assembly)
            .Pipelines(p => p.ForAsks().UseValidation())
        );
        services.AddValidatorsFromAssemblyContaining<AskPingValidator>();

        await using var sp = services.BuildServiceProvider();
        
        var synq = sp.GetRequiredService<ISynq>();

        Func<Task> act = async () => _ = await synq.Dispatch(new AskPing("x"));

        await act.Should().ThrowAsync<SynqValidationException>()
            .Where(e => e.Errors.ContainsKey(nameof(AskPing.Payload)));
    }
}