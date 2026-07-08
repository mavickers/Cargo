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

- Versioning is managed by Nerdbank.GitVersioning (`version.json`). Only bump major/minor manually; patch auto-increments from commit count.
- After a PR is merged, ask the user if they want to tag a release.
- To release: build to get the Nerdbank version (`dotnet build Cargo/Cargo.csproj --configuration Release`, check the `.nupkg` filename), tag with that version, and push the tag. The GitHub Action handles the NuGet publish.

## Test Organization

- `Integration/` — Full pipeline scenario tests
- `Unit/` — Isolated component tests
- Station implementations for tests are nested classes within test files
