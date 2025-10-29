using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace FQ.SynQ.Caching.UnitTests;

public class AskCachingTests
{
    [Fact]
    public async Task Caches_Query_Result_And_Replays()
    {
        using var sp = TestHost.Build(b => b
            .ScanHandlers(typeof(AskNowHandler).Assembly)
            .UseCaching()
        );

        var synq = sp.GetRequiredService<ISynq>();
        var calls = sp.GetRequiredService<CallCounter>();

        calls.Reset();

        var first = await synq.Dispatch(new AskNow("k1"));
        var second = await synq.Dispatch(new AskNow("k1"));

        first.Should().NotBeNullOrEmpty();
        second.Should().Be(first);
        calls.Calls.Should().Be(1);
    }

    [Fact]
    public async Task PerMessage_Bypass_Disables_Cache()
    {
        using var sp = TestHost.Build(b => b
            .ScanHandlers(typeof(AskNowHandler).Assembly)
            .UseCaching()
        );

        var synq = sp.GetRequiredService<ISynq>();
        var calls = sp.GetRequiredService<CallCounter>();

        calls.Reset();

        var a = await synq.Dispatch(new AskNow("k2", Bypass: true));
        var b = await synq.Dispatch(new AskNow("k2", Bypass: true));

        a.Should().NotBe(b);
        calls.Calls.Should().Be(2);
    }

    [Fact]
    public async Task Global_Bypass_Disables_Cache()
    {
        using var sp = TestHost.Build(b => b
            .ScanHandlers(typeof(AskNowHandler).Assembly)
            .UseCaching(),
            more: services =>
            {
                services.PostConfigure<AskCacheOptions>(o =>
                {
                    o.GlobalBypass = _ => true;
                });
            });

        var synq = sp.GetRequiredService<ISynq>();
        var calls = sp.GetRequiredService<CallCounter>();

        calls.Reset();

        var a = await synq.Dispatch(new AskNow("k3"));
        var b = await synq.Dispatch(new AskNow("k3"));

        a.Should().NotBe(b);
        calls.Calls.Should().Be(2);
    }

    [Fact]
    public async Task Partition_Isolates_Cache_Entries()
    {
        using var sp = TestHost.Build(b => b
            .ScanHandlers(typeof(AskNowHandler).Assembly)
            .UseCaching()
        );

        var synq = sp.GetRequiredService<ISynq>();
        var part = (TestPartitionAccessor)sp.GetRequiredService<ICachePartitionAccessor>();
        var calls = sp.GetRequiredService<CallCounter>();

        calls.Reset();

        part.Set("tenant:A");
        var a1 = await synq.Dispatch(new AskNow("kP"));
        var a2 = await synq.Dispatch(new AskNow("kP"));

        part.Set("tenant:B");
        var b1 = await synq.Dispatch(new AskNow("kP"));
        var b2 = await synq.Dispatch(new AskNow("kP"));

        a2.Should().Be(a1);
        b2.Should().Be(b1);
        b1.Should().NotBe(a1);
        calls.Calls.Should().Be(2);
    }

    [Fact]
    public async Task StampedeProtection_Ensures_Single_Execution()
    {
        await using var sp = TestHost.Build(b => b
            .ScanHandlers(typeof(AskNowHandler).Assembly)
            .UseCaching(),
            more: services =>
            {
                services.PostConfigure<AskCacheOptions>(o =>
                {
                    o.StampedeProtection = true;
                });
            });

        var synq = sp.GetRequiredService<ISynq>();
        var calls = sp.GetRequiredService<CallCounter>();
        
        calls.Reset();

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => synq.Dispatch(new AskNow("kS")))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        results.Should().OnlyContain(r => r == results[0]);
        calls.Calls.Should().Be(1);
    }

    [Fact]
    public async Task CacheNulls_False_Does_Not_Cache_Nulls()
    {
        await using var sp = TestHost.Build(b => b
            .ScanHandlers(typeof(AskNullHandler).Assembly)
            .UseCaching()
        );

        var synq = sp.GetRequiredService<ISynq>();
        var calls = sp.GetRequiredService<CallCounter>();
        calls.Reset();

        var r1 = await synq.Dispatch(new AskNull("kN", CacheNull: false));
        var r2 = await synq.Dispatch(new AskNull("kN", CacheNull: false));

        r1.Should().BeNull();
        r2.Should().BeNull();
        calls.Calls.Should().Be(2);
    }

    [Fact]
    public async Task CacheNulls_True_Caches_Nulls()
    {
        await using var sp = TestHost.Build(b => b
            .ScanHandlers(typeof(AskNullHandler).Assembly)
            .UseCaching()
        );

        var synq = sp.GetRequiredService<ISynq>();
        var calls = sp.GetRequiredService<CallCounter>();
        calls.Reset();

        var r1 = await synq.Dispatch(new AskNull("kN2", CacheNull: true));
        var r2 = await synq.Dispatch(new AskNull("kN2", CacheNull: true));

        r1.Should().BeNull();
        r2.Should().BeNull();
        calls.Calls.Should().Be(1);
    }

    [Fact]
    public async Task Ttl_Expiry_Evicts_Entry()
    {
        await using var sp = TestHost.Build(b => b
            .ScanHandlers(typeof(AskNowHandler).Assembly)
            .UseCaching(),
            more: services =>
            {
                services.PostConfigure<AskCacheOptions>(o =>
                {
                    o.DefaultTtl = TimeSpan.FromSeconds(1);
                });
            });

        var synq = sp.GetRequiredService<ISynq>();
        var calls = sp.GetRequiredService<CallCounter>();
        var clock = (FakeClock)sp.GetRequiredService<IClock>();
        calls.Reset();

        var r1 = await synq.Dispatch(new AskNow("kTTL"));
        clock.Advance(TimeSpan.FromSeconds(2));
        var r2 = await synq.Dispatch(new AskNow("kTTL"));

        r2.Should().NotBe(r1);
        calls.Calls.Should().Be(2);
    }
}