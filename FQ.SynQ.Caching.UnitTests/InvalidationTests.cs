using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ.Caching.UnitTests;

public class InvalidationTests
{
    [Fact]
    public async Task Invalidate_By_Tag_Refreshes_Cache()
    {
        await using var sp = TestHost.Build(b => b
            .ScanHandlers(typeof(AskNowHandler).Assembly, typeof(ActTouchHandler).Assembly)
            .UseCaching()
            .UseCacheInvalidation()
        );

        var synq = sp.GetRequiredService<ISynq>();
        var calls = sp.GetRequiredService<CallCounter>();
        calls.Reset();

        var q1 = await synq.Dispatch(new AskNow("kT"));
        var q2 = await synq.Dispatch(new AskNow("kT"));

        q2.Should().Be(q1);
        calls.Calls.Should().Be(1);

        var tag = $"tag:kT";
        _ = await synq.Dispatch(new ActTouch(tag));

        var q3 = await synq.Dispatch(new AskNow("kT"));

        q3.Should().NotBe(q1);
        calls.Calls.Should().Be(2);
    }

    [Fact]
    public async Task Invalidate_By_Key_Refreshes_Cache()
    {
        await using var sp = TestHost.Build(b => b
            .ScanHandlers(typeof(AskNowHandler).Assembly, typeof(ActTouchHandler).Assembly)
            .UseCaching()
            .UseCacheInvalidation()
        );

        var synq = sp.GetRequiredService<ISynq>();
        var calls = sp.GetRequiredService<CallCounter>();
        var part = (TestPartitionAccessor)sp.GetRequiredService<ICachePartitionAccessor>();
        part.Set("tenant:X");
        calls.Reset();

        var r1 = await synq.Dispatch(new AskNow("kK"));
        var fullKey = TestHost.BuildFullQueryKey<AskNow>("kK", "tenant:X");

        _ = await synq.Dispatch(new ActTouch(Tag: "ignored", FullKey: fullKey));

        var r2 = await synq.Dispatch(new AskNow("kK"));

        r2.Should().NotBe(r1);
        calls.Calls.Should().Be(2);
    }
}