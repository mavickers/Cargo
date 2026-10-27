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
- **Station repeat** — stations can repeat themselves until a condition is met, with an optional delay and a per-station limit
- **Final station** — a station that always runs, even after an abort
- **Observers** — receive a callback as each station starts and finishes, with iteration and elapsed time
- **Cancellation** — pass a `CancellationToken` to `GoAsync()`, accessible via `Package.CancellationToken`. Optionally treat cancellation as a graceful abort instead of an exception.
- **Tracing** — built-in trace messages for pipeline execution
- **Result tracking** — inspect results from each station via `Package.Results`
- **Broad compatibility** — targets `netstandard2.0` (runs on .NET Framework 4.6.1+, .NET Core 2.0+, and .NET 5/6/7/8+)

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

A station constructor that throws is handled the same way as a `Process()` that throws: the station records a failed result and the pipeline follows the abort-on-error setting, so the final station still runs.

Registration is validated up front. `WithStation<T>()` and `WithFinalStation<T>()` throw `ArgumentException` when the type does not derive from `Station<T>`/`StationAsync<T>` for the bus's content type, is abstract, or has no public parameterless constructor. `WithStations(params Type[])` skips a type that is not a station at all, but still throws for a station that is abstract, open generic, has no parameterless constructor, or has the wrong content type.

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

Set a repeat limit to prevent infinite loops (default is 100). When the limit is reached, the station records a failed result with an `OverflowException` and the pipeline follows the abort-on-error setting:

```csharp
var bus = Bus.New<OrderContext>()
    .WithStationRepeatLimit(10)
    .WithStation<RetryableStation>();
```

A station can set its own limit by overriding `RepeatLimit`. The override replaces the bus-wide value for that station only; returning `null` keeps the bus-wide value. It is read from a new instance on every run, after `Package` is set, so it can depend on the content:

```csharp
public class PollStation : Station<OrderContext>
{
    protected override int? RepeatLimit => Package.Contents.MaxPolls;

    public override Station.Action Process() { ... }
}
```

To wait before the next attempt, add a delay to the repeat action. The bus waits (honoring the cancellation token) and then runs the station again. The delay is ignored on `Next` and `Abort`. A delay also works under the synchronous `Go()`, which blocks the calling thread for the delay; the rest of the run then continues on a thread-pool thread, not the calling thread:

```csharp
return Station.Action.Repeat("Not ready").WithDelay(TimeSpan.FromSeconds(5));
```

Cancellation during the delay is handled like cancellation between stations: by default `OperationCanceledException` is thrown; with `WithAbortOnCancel()` an abort result is recorded and the final station runs. The final station runs once after cancellation; if it returns `Repeat`, the repeat is recorded as the abort result instead of running again.

### Observing Stations

To react as each station starts and finishes — for live progress output, logging, or timing — register an observer:

```csharp
public class ConsoleObserver : IStationObserver
{
    public void Starting(Type station, int iteration)
        => Console.WriteLine($"{station.Name} (attempt {iteration})");

    public void Finished(Station.Result result)
        => Console.WriteLine($"  {(result.WasSuccess ? "ok" : "failed")} in {result.Elapsed.TotalMilliseconds:F0} ms");
}

var bus = Bus.New<OrderContext>()
    .WithStationObserver(new ConsoleObserver())
    .WithStation<FetchStation>()
    .WithStation<DeployStation>();
```

`Starting` fires before each run; `iteration` starts at 1 and counts repeats of the same station. `Finished` fires after each result is recorded, including the abort result that `WithAbortOnCancel()` records for a cancelled station (for which `Starting` does not fire, and whose `Elapsed` is zero). `Starting` also fires for the attempt that exceeds the repeat limit; that attempt records the `OverflowException` result without running `Process()`. Every `Station.Result` carries `Iteration` and `Elapsed`; `Elapsed` is measured from after `Starting` returns and includes station construction.

The observer runs outside the station's error handling. An exception thrown by the observer escapes `Go()`/`GoAsync()` unchanged; it is never recorded as a station failure.

### Cancellation

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

var result = await Bus.New<OrderContext>()
    .WithStation<SlowStation>()
    .GoAsync(context, cts.Token);

// Inside a station, access the token:
// Package.CancellationToken
```

Cancellation is checked between stations. By default, a cancelled token throws `OperationCanceledException` and skips all remaining stations, including the final station.

To treat cancellation as a graceful abort instead (record an abort result, run the final station, return normally):

```csharp
var bus = Bus.New<OrderContext>()
    .WithAbortOnCancel()
    .WithStation<SlowStation>()
    .WithFinalStation<CleanupStation>();

await bus.GoAsync(context, cts.Token);

// No exception thrown — inspect the package state instead:
if (bus.Package.IsAborted) { /* handle the graceful cancellation */ }
```

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
| `.WithAbortOnCancel()` | Treat cancellation as a graceful abort instead of throwing |
| `.WithNoAbortOnCancel()` | Throw on cancellation (default) |
| `.WithStationRepeatLimit(int)` | Set max repeat iterations (default: 100) |
| `.WithStationObserver(IStationObserver)` | Receive a callback as each station starts and finishes |
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
| `.WithMessage(string)` | Attach a message to any action |
| `.WithException(Exception)` | Attach an exception to any action |
| `.WithDelay(TimeSpan)` | Wait before the repeat runs the station again; zero to `int.MaxValue` milliseconds, otherwise `ArgumentOutOfRangeException` |
| `ActionType` / `ActionMessage` / `ActionException` / `Delay` | Read back what was attached |

### Station.Result

| Member | Description |
|--------|-------------|
| `Station` | The station type that produced the result |
| `WasSuccess` / `WasFailure` | Whether the station completed or threw |
| `IsAborting` / `IsRepeating` | Whether the returned action was `Abort` or `Repeat` |
| `ActionMessage` / `ActionException` | The message or exception attached to the action |
| `ActionDelay` | The delay attached to a `Repeat` action |
| `Exception` | The exception that failed the station, if any |
| `Iteration` | 1 for a first run, higher on each repeat of the same station |
| `Elapsed` | Wall-clock time for the run, including construction; zero for a cancellation abort result |

### Station Members

| Member | Description |
|--------|-------------|
| `Package` | The current package |
| `Package.Contents` | The content model |
| `Package.CancellationToken` | The cancellation token (async pipelines) |
| `IsErrored` | Whether any station has errored |
| `LastResult` | The result from the previous station |
| `PackageResults` | All station results so far |
| `RepeatLimit` | Override to set this station's repeat limit; `null` (default) uses the bus-wide limit. A station member of the same name that is not an override hides it and is ignored by the bus |
| `Messages` | Trace messages |
| `GetService<T>()` | Get a registered service |
| `HasService<T>()` | Check if a service is registered |
| `TryGetService<T>(out T)` | Safely get a service |
| `Trace(string)` | Add a trace message |

## Target Frameworks

The library targets `netstandard2.0`, so it runs on any compatible runtime, including:

- .NET Framework 4.6.1+
- .NET Core 2.0+
- .NET 5, 6, 7, 8 and later

## License

[MIT](Cargo/license.md) - LightPath Solutions, LLC
