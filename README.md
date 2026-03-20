# Cargo

A lightweight Chain of Responsibility pipeline library for .NET.

[![NuGet](https://img.shields.io/nuget/v/LightPath.Cargo)](https://www.nuget.org/packages/LightPath.Cargo)

## Why Would I Use This?

You have a series of steps that process or transform data — validation, enrichment, persistence, notification — and you want each step to be:

- **Isolated** — each step is its own class with a single responsibility
- **Composable** — mix, match, and reorder steps without touching their internals
- **Testable** — test each step independently
- **Flexible** — support sync and async work, error handling, retries, and early termination

Cargo gives you this with minimal ceremony. Define your steps as stations, wire them into a bus, and go.

**Common use cases:**
- Request processing pipelines
- Validation chains
- ETL / data transformation
- Workflow orchestration
- Multi-step form processing

## Installation

```
dotnet add package LightPath.Cargo
```

## Quick Start

Define a content model (the data that flows through the pipeline):

```csharp
public class OrderContext
{
    public string CustomerId { get; set; }
    public decimal Total { get; set; }
    public bool IsValid { get; set; }
    public bool IsProcessed { get; set; }
}
```

Create stations (each step in the pipeline):

```csharp
public class ValidateOrder : Station<OrderContext>
{
    public override Station.Action Process()
    {
        Package.Contents.IsValid = Package.Contents.Total > 0
                                && !string.IsNullOrEmpty(Package.Contents.CustomerId);

        return Package.Contents.IsValid
            ? Station.Action.Next()
            : Station.Action.Abort("Invalid order");
    }
}

public class ProcessOrder : Station<OrderContext>
{
    public override Station.Action Process()
    {
        Package.Contents.IsProcessed = true;
        return Station.Action.Next();
    }
}
```

Wire them up and run:

```csharp
var context = new OrderContext { CustomerId = "C-123", Total = 49.99m };

var result = Bus.New<OrderContext>()
    .WithStation<ValidateOrder>()
    .WithStation<ProcessOrder>()
    .Go(context);

// context.IsValid == true
// context.IsProcessed == true
```

## Features

- **Sync and async stations** — `Station<T>` for synchronous work, `StationAsync<T>` for async/IO-bound work
- **Mixed pipelines** — freely interleave sync and async stations in the same pipeline
- **Services** — register and inject dependencies into stations
- **Error handling** — abort on error (default) or continue with error state
- **Station repeat** — stations can repeat themselves until a condition is met
- **Final station** — a station that always runs, even after an abort
- **Cancellation** — pass a `CancellationToken` to `GoAsync()`, accessible via `Package.CancellationToken`
- **Tracing** — built-in trace messages for pipeline execution
- **Result tracking** — inspect results from each station via `Package.Results`
- **Multi-target** — supports net472, net48, net6.0, net7.0, net8.0

## Core Concepts

**Package** — wraps your content model and carries it through the pipeline. Holds results, services, trace messages, and error state.

**Station** — a single step in the pipeline. Receives the package, does its work, and returns an action: `Next()`, `Abort()`, or `Repeat()`.

**Bus** — builds and executes the pipeline. You register stations, services, and configuration, then call `Go()` or `GoAsync()`.

## Examples

### Async Stations

```csharp
public class FetchCustomerData : StationAsync<OrderContext>
{
    public override async Task<Station.Action> ProcessAsync()
    {
        var service = GetService<ICustomerApi>();
        var customer = await service.GetCustomerAsync(Package.Contents.CustomerId);

        Package.Contents.CustomerName = customer.Name;
        return Station.Action.Next();
    }
}

var result = await Bus.New<OrderContext>()
    .WithService<ICustomerApi>(customerApi)
    .WithStation<ValidateOrder>()
    .WithStation<FetchCustomerData>()
    .WithStation<ProcessOrder>()
    .GoAsync(context);
```

Async stations run with the caller's synchronization context. Cargo uses `ConfigureAwait(false)` internally, but station authors control their own awaits — standard .NET async guidance applies.

### Services

Register dependencies and access them from any station:

```csharp
var bus = Bus.New<OrderContext>()
    .WithService<ICustomerApi>(customerApi)
    .WithService<ILogger>(logger)
    .WithStation<FetchCustomerData>();

// Inside a station:
var api = GetService<ICustomerApi>();
var logger = GetService<ILogger>();

// Or safely:
if (TryGetService<ILogger>(out var logger))
{
    logger.Log("Processing order");
}
```

### Error Handling

By default, an exception in a station aborts the pipeline:

```csharp
var bus = Bus.New<OrderContext>()
    .WithStation<RiskyStation>()
    .WithStation<NextStation>()
    .WithFinalStation<CleanupStation>();

// If RiskyStation throws, NextStation is skipped, CleanupStation runs.
```

To continue after errors:

```csharp
var bus = Bus.New<OrderContext>()
    .WithNoAbortOnError()
    .WithStation<RiskyStation>()
    .WithStation<NextStation>();

// If RiskyStation throws, NextStation still runs.
// Check Package.IsErrored or LastResult.WasFailure to inspect error state.
```

### Station Repeat

A station can repeat itself until a condition is met:

```csharp
public class RetryableStation : Station<OrderContext>
{
    public override Station.Action Process()
    {
        Package.Contents.Attempts++;

        if (Package.Contents.Attempts < 3)
            return Station.Action.Repeat();

        return Station.Action.Next();
    }
}
```

Set a repeat limit to prevent infinite loops (default is 100):

```csharp
var bus = Bus.New<OrderContext>()
    .WithStationRepeatLimit(10)
    .WithStation<RetryableStation>();
```

### Cancellation

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

var result = await Bus.New<OrderContext>()
    .WithStation<SlowStation>()
    .GoAsync(context, cts.Token);

// Inside a station, access the token:
// Package.CancellationToken
```

Cancellation is checked between stations. If the token is cancelled, the pipeline throws `OperationCanceledException` and skips all remaining stations, including the final station.

### Abort with Messages

```csharp
return Station.Action.Abort("Payment declined");

// Later:
bus.Package.AbortMessage // "Payment declined"
bus.Package.IsAborted    // true
```

## API Reference

### Bus

| Method | Description |
|--------|-------------|
| `Bus.New<T>()` | Create a new bus |
| `.WithStation<TStation>()` | Add a station to the pipeline |
| `.WithStations(params Type[])` | Add multiple stations by type |
| `.WithFinalStation<TStation>()` | Set a station that runs after abort or error |
| `.WithService<T>(T service)` | Register a typed service |
| `.WithServices(strategy, params object[])` | Register multiple services |
| `.WithAbortOnError()` | Abort pipeline on station error (default) |
| `.WithNoAbortOnError()` | Continue pipeline on station error |
| `.WithStationRepeatLimit(int)` | Set max repeat iterations (default: 100) |
| `.Go(content, callback?)` | Execute the pipeline synchronously |
| `.GoAsync(content, token?, callback?)` | Execute the pipeline asynchronously |

### Station.Action

| Method | Description |
|--------|-------------|
| `Next()` | Proceed to the next station |
| `Next(string)` | Proceed with a message |
| `Next(Exception)` | Proceed with an exception |
| `Abort()` | Abort the pipeline |
| `Abort(string)` | Abort with a message |
| `Abort(Exception)` | Abort with an exception |
| `Repeat()` | Repeat the current station |
| `Repeat(string)` | Repeat with a message |
| `Repeat(Exception)` | Repeat with an exception |

### Station Members

| Member | Description |
|--------|-------------|
| `Package` | The current package |
| `Package.Contents` | The content model |
| `Package.CancellationToken` | The cancellation token (async pipelines) |
| `IsErrored` | Whether any station has errored |
| `LastResult` | The result from the previous station |
| `PackageResults` | All station results so far |
| `Messages` | Trace messages |
| `GetService<T>()` | Get a registered service |
| `HasService<T>()` | Check if a service is registered |
| `TryGetService<T>(out T)` | Safely get a service |
| `Trace(string)` | Add a trace message |

## Target Frameworks

- .NET Framework 4.7.2
- .NET Framework 4.8
- .NET 6.0
- .NET 7.0
- .NET 8.0

## License

[MIT](Cargo/license.md) - LightPath Solutions, LLC
