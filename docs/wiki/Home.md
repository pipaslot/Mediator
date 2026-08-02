See release changes [here](Release-notes-and-breaking-changes).

Pipaslot.Mediator implements the Mediator pattern for .NET with an ASP.NET Core-style middleware pipeline: a single, consistent entry point for your business actions instead of scattering calls across services, with cross-cutting concerns (validation, authorization, logging, transactions, ...) centralized once in middleware instead of duplicated per handler.

Its distinguishing feature: the exact same action and handler code can run **in-process**, or be transported **over HTTP** without changing a single line - which makes it a great fit for Blazor WASM client/server apps sharing one codebase between client and server.

```csharp
// Shared project
public record WeatherForecastRequest : IRequest<WeatherForecastResult[]>
{
    public DateTime Since { get; init; } = DateTime.Now;
    public uint AmountOfDays { get; init; } = 1;
}

// Executable project
public class WeatherForecastRequestHandler : IRequestHandler<WeatherForecastRequest, WeatherForecastResult[]>
{
    public Task<WeatherForecastResult[]> Handle(WeatherForecastRequest request, CancellationToken cancellationToken)
        => Task.FromResult(/* ... */);
}

// Anywhere IMediator is injected
var response = await mediator.Execute(new WeatherForecastRequest());
```

## Where to start

Pick the path that matches what you're doing:

- **Just evaluating the library?** Read [Why Pipaslot.Mediator](1.-Why-Pipaslot.Mediator.md) for when to use it (and when not to), and how it compares to a plain SOA-style service layer.
- **Building a single .NET app** (API, console, ...)? Jump straight to [Quickstart: In-process usage](3.-Quickstart-In-process-usage.md).
- **Building a Blazor WASM client/server app**? Jump straight to [Quickstart: Client-Server (Blazor WASM) usage](4.-Quickstart-Client-Server-Blazor-WASM-usage.md). A runnable example application (Server + Client Blazor WASM + Shared) is available in the [`Demo/`](https://github.com/pipaslot/Mediator/tree/main/Demo) folder of the repository.
- **Deciding what a failing action should report to the caller?** See [Exception handling](6.2.-Exception-handling.md) for the safe-by-default behavior and how to register your own exception handlers.
- **Upgrading an existing installation?** Check [Release notes and breaking changes](Release-notes-and-breaking-changes.md).
- **Reading end to end?** Follow the chapters in order using the sidebar navigation, starting with [Why Pipaslot.Mediator](1.-Why-Pipaslot.Mediator.md).
