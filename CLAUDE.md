Cargo is a lightweight Chain of Responsibility pipeline library for .NET.

## Build & Test

- `dotnet build Cargo.sln`
- `dotnet test Cargo.sln`
- NuGet package: https://www.nuget.org/packages/LightPath.Cargo

## Project Layout

- `Cargo/` — Main library (namespace: `LightPath.Cargo`, assembly: `LightPath.Cargo`)
- `Cargo.Tests/` — Test project (xUnit, FluentAssertions, Moq, coverlet)
- `Cargo/` targets netstandard2.0 (single target — broad reach: .NET Framework 4.6.1+, .NET 6/7/8+)
- `Cargo.Tests/` multi-targets runnable frameworks: net472, net48, net6.0, net7.0, net8.0

## Architecture Notes

- Pipeline supports both synchronous (`Go()`) and asynchronous (`GoAsync()`) execution.
- `Go()` throws `InvalidOperationException` if the pipeline contains async stations.
- Stations inherit from `Station<T>` (sync) or `StationAsync<T>` (async), both sharing `StationBase<T>`.
- The bus reaches into stations by reflection (`_package`, `RepeatLimit`, `Process`/`ProcessAsync`); renaming those members breaks `GoAsync` at run time, not compile time.
- `WithStation`/`WithFinalStation` validate the type at registration (content type, concrete, parameterless ctor); `WithStations(params Type[])` skips invalid types silently. A new station instance is constructed inside the try on every run, so a throwing constructor is a Failed result, not an escaped exception.
- `IStationObserver` callbacks run outside the station try; an observer exception escapes `Go`/`GoAsync`.

## Releasing

- Versioning is managed by Nerdbank.GitVersioning (`version.json`). Only bump major/minor manually (edit `version.json`'s `version` field); patch auto-increments from commit height. Off-master builds carry a `-g<sha>` prerelease suffix; master builds (per `publicReleaseRefSpec`) are clean.
- **Publishing is fully automated.** Every merge to `master` triggers `.github/workflows/publish.yml`, which builds, tests (net8.0), packs, pushes to nuget.org (`--skip-duplicate`), then tags the commit with the published version and pushes the tag. Master is fed only by release-worthy PR merges — so merging a PR is the release.
- No manual tagging or local push needed. To cut a release: merge the PR to master. To do a major/minor bump, land a `version.json` edit in that PR.
- The push uses the `NUGET_API_KEY` repository secret (nuget.org keys expire after at most one year; regenerate on nuget.org and update the secret when the publish job fails on the push step).

## Test Organization

- `Integration/` — Full pipeline scenario tests
- `Unit/` — Isolated component tests
- Station implementations for tests are nested classes within test files
