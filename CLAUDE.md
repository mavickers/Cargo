Cargo is a lightweight Chain of Responsibility pipeline library for .NET.

## Build & Test

- `dotnet build Cargo.sln`
- `dotnet test Cargo.sln`
- `publish.bat -local [version]` — publish to local NuGet feed (`d:\Local Packages`)
- `publish.bat -nuget [version]` — publish to nuget.org
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

## Releasing

- Versioning is managed by Nerdbank.GitVersioning (`version.json`). Only bump major/minor manually (edit `version.json`'s `version` field); patch auto-increments from commit height. Off-master builds carry a `-g<sha>` prerelease suffix; master builds (per `publicReleaseRefSpec`) are clean.
- **Publishing is fully automated.** Every merge to `master` triggers `.github/workflows/publish.yml`, which builds, tests (net8.0), packs, pushes to nuget.org (`--skip-duplicate`), then tags the commit with the published version and pushes the tag. Master is fed only by release-worthy PR merges — so merging a PR is the release.
- No manual tagging or `publish.bat` needed. To cut a release: merge the PR to master. To do a major/minor bump, land a `version.json` edit in that PR.

## Test Organization

- `Integration/` — Full pipeline scenario tests
- `Unit/` — Isolated component tests
- Station implementations for tests are nested classes within test files
