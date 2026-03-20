Cargo is a lightweight Chain of Responsibility pipeline library for .NET.

## Build & Test

- `dotnet build Cargo.sln`
- `dotnet test Cargo.sln`
- `publish.bat -local [version]` — publish to local NuGet feed (`d:\Local Packages`)
- `publish.bat -nuget [version]` — publish to nuget.org

## Project Layout

- `Cargo/` — Main library (namespace: `LightPath.Cargo`, assembly: `LightPath.Cargo`)
- `Cargo.Tests/` — Test project (xUnit, FluentAssertions, Moq, coverlet)
- Both projects multi-target: net472, net48, net6.0, net7.0, net8.0

## Architecture Notes

- Pipeline is entirely synchronous — no async/await.

## Test Organization

- `Integration/` — Full pipeline scenario tests
- `Unit/` — Isolated component tests
- Station implementations for tests are nested classes within test files
