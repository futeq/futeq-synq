# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository

`FQ.SynQ` — a CQRS / mediator + pipeline framework for .NET. The solution (`FQ.SynQ.sln`) contains three NuGet libraries under `src/` and matching xUnit test projects under `tests/`. Source-of-truth NuGet metadata lives in each `*.csproj`; the package id uses `FQ.Synq*` (note casing diverges from folder names `FQ.SynQ*`).

Libraries multi-target `net8.0;net9.0`; test projects target `net9.0` only. CI uses .NET 9 SDK.

## Common commands

```bash
# Restore + build everything
dotnet restore
dotnet build -c Release

# Run all tests
dotnet test -c Release

# Run a single test project
dotnet test tests/FQ.SynQ.UnitTests/FQ.SynQ.UnitTests.csproj

# Run a single test by fully-qualified name (or substring)
dotnet test --filter "FullyQualifiedName~SynqDispatcherDispatchesToHandler"

# Pack (mirrors what semantic-release runs in CI)
NEXT_VERSION=0.0.0-local ./scripts/pack-nuget.sh
```

There is no separate lint step; rely on `dotnet build` (warnings) and `Nullable=enable` enforcement.

## Architecture

Three layers — keep changes scoped to the layer that owns the concern.

### `FQ.SynQ` (core, zero domain-specific filters)
- `IMessage<TOut>` is the root marker. `IAct` / `IAct<TOut>` mark commands; `IAsk<TOut>` marks queries. The split is what `FilterCatalog` uses to decide which pipeline applies — *do not* dispatch on type names.
- `ISynq.Dispatch` is implemented by `SynqDispatcher` (`src/FQ.SynQ/Dispatchers/SynqDispatcher.cs`). It creates a DI scope per dispatch, resolves the handler via `IMessageHandler<TMsg,TOut>`, then wraps it in filters by walking the catalog **in reverse** so the first registered filter ends up outermost. Reflection is used to invoke both handler and filters; `TargetInvocationException` is unwrapped via `ExceptionDispatchInfo` — preserve that behavior when editing the dispatcher so user exceptions surface intact.
- `SynqBuilder` (`src/FQ.SynQ/SynqBuilder.cs`) is the configuration entry point. `ScanHandlers(asm)` registers any concrete `IMessageHandler<,>` it finds as transient. `Pipelines(cfg => …)` configures the `FilterCatalogConfig` via `PipelineConfigurator`.
- `FilterCatalogConfig` has four buckets that compose at dispatch time, in this order: `GlobalFilters` → `ActFilters` or `AskFilters` (whichever matches) → `PredicateRules` → `PerMessage`. Adding a new bucket requires updating `FilterCatalog.CreateFilters` to keep the order explicit.
- Filters are open generic types (e.g. `typeof(MyFilter<,>)`) closed at dispatch via `MakeGenericType(messageType, outType)` and instantiated with `ActivatorUtilities`. New built-in filters must be open generic with the `IFilter<T,TOut> where T : IMessage<TOut>` shape.

### `FQ.SynQ.Filters` (cross-cutting filters)
- One folder per filter family (`Authorization`, `Idempotency`, `Performance`, `UnitOfWork`, `Validation`, `DomainEvents`).
- Public API for consumers is the extension methods in `ConfigurationExtensions.*.cs` (`UseValidation`, `UseAuthorization`, etc.) plus the presets in `SynqBuilderExtensions.cs` (`AddCommonFilters`, `AddWebApiDefaults`, `AddStrictReadWriteDefaults`, `AddPredicatePreset`). Filter classes themselves are typically internal — keep them so unless you have a specific reason to expose.
- Conventional pipeline order (encoded in the presets, mirrored in README): commands run `Validation → Authorization → Idempotency → UnitOfWork → Handler → DomainEvents → CacheInvalidation`; queries run `Validation → Authorization → QueryCache → Handler`. When adding a new filter, slot it consistent with this ordering and update the relevant preset rather than only documenting it.
- Validation depends on `FluentValidation` 12; that's the only third-party runtime dep in this project beyond Microsoft.Extensions.

### `FQ.SynQ.Caching`
- `CacheFilter<T,TOut>` only activates when `T` implements `IAsk<>` *and* `ICacheable`; otherwise it's a no-op pass-through. `CacheInvalidationFilter<T,TOut>` is the write-side counterpart for `IAct`.
- Cache keys are namespaced as `synq:q:{partition?}|{messageType.FullName}|{userKey}`. Keep this format stable — changing it invalidates every cached entry across deployments.
- `AddCachingPreset` in `src/FQ.SynQ.Caching/SynqBuilderExtensions.cs` reaches into `SynqBuilder` via reflection on the private `_services` field to call `AddSynqCaching`. This is a deliberate workaround so `SynqBuilder` stays minimal in the core package; if you change the field name or surface a public accessor, update this extension at the same time.
- Consumers must register an `ICacheStore` implementation themselves; only the serializer (`SystemTextJsonCacheSerializer`) is provided by default.

## Release flow

- Branching model is **GitHub Flow**: `main` is the only long-lived branch; work happens on `feature/**` (or `hotfix/**`) branches and merges back to `main` via PR.
- Versioning is driven by **GitVersion** (`.github/GitVersion.yml`, `workflow: GitHubFlow/v1`). The seed `next-version: 1.0.0` controls the next stable release; bump it there when you want a major/minor jump. Builds on `main` produce stable versions; `feature/**` builds get `alpha.{BranchName}` labels; PR builds get `PullRequest` labels. Don't hand-edit `Version` in csproj files.
- `.github/workflows/release.yml` runs on every push to `main`/`feature/**`/`hotfix/**` and on PRs to `main`. It (1) runs GitVersion via `gittools/actions`, (2) builds + tests with the computed version baked in via `/p:Version`, (3) on non-PR pushes calls `scripts/pack-nuget.sh` with `NEXT_VERSION=${{ gitversion.nuGetVersionV2 }}`, (4) only on `main` calls `scripts/publish-nuget.sh` (pushes `artifacts/*.nupkg` with `--skip-duplicate`) and creates a `vX.Y.Z` tag + GitHub Release.
- Conventional Commits are still recommended (`.gitmessage.txt` lists the allowed types) but no longer drive versioning — increments come from GitVersion's branch config.

## Testing notes

- xUnit + FluentAssertions across all three test projects. `TestFixtures.cs` in each project contains shared message/handler stubs — prefer reusing those over inventing new ones for a single test.
- Filter ordering is covered specifically by `tests/FQ.SynQ.UnitTests/SynqFilterOrderingTests.cs`; if you change `FilterCatalog.CreateFilters` or the dispatcher's wrapping loop, run that file first.
