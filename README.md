# Pipaslot.Mediator

[![NuGet: Pipaslot.Mediator](https://img.shields.io/nuget/v/Pipaslot.Mediator?label=Pipaslot.Mediator)](https://www.nuget.org/packages/Pipaslot.Mediator/)
[![NuGet: Pipaslot.Mediator.Http](https://img.shields.io/nuget/v/Pipaslot.Mediator.Http?label=Pipaslot.Mediator.Http)](https://www.nuget.org/packages/Pipaslot.Mediator.Http/)
[![CI](https://github.com/pipaslot/Mediator/actions/workflows/ci.yml/badge.svg)](https://github.com/pipaslot/Mediator/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Mediator pattern for .NET with an ASP.NET Core-style middleware pipeline. Define an action contract and its handler once - the exact same code runs **in-process**, or is transported **over HTTP** without changing a single line, which makes it a great fit for Blazor WASM client/server apps sharing one codebase.

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

Register it with `services.AddMediator()` for in-process use, or `AddMediatorClient()` / `AddMediatorServer()` (`Pipaslot.Mediator.Http`) to run the same action/handler across a client-server boundary over HTTP.

## Key features

- Single, consistent entry point (`IMediator.Execute`/`Dispatch`) instead of scattering calls across services
- ASP.NET Core-style middleware pipeline for cross-cutting concerns (validation, logging, transactions, ...)
- Declarative authorization (policies, `[RolePolicy]`, per-action or global) evaluated before the handler runs
- Pub/sub notifications that propagate through nested mediator calls
- The same action/handler runs in-process or over HTTP for Blazor WASM client/server apps, with no code duplication

## Getting started

- New to the library? Start with [Why Pipaslot.Mediator](https://github.com/pipaslot/Mediator/wiki/1.-Why-Pipaslot.Mediator) for the reasoning and trade-offs.
- Building a single .NET app (API, console, ...)? Go to [Quickstart: In-process usage](https://github.com/pipaslot/Mediator/wiki/3.-Quickstart-In-process-usage).
- Building a Blazor WASM client/server app? Go to [Quickstart: Client-Server (Blazor WASM) usage](https://github.com/pipaslot/Mediator/wiki/4.-Quickstart-Client-Server-Blazor-WASM-usage), or check the runnable [Demo](Demo/) in this repo.
- Full documentation: [Wiki](https://github.com/pipaslot/Mediator/wiki)
- Upgrading? See [Release notes and breaking changes](https://github.com/pipaslot/Mediator/wiki/Release-notes-and-breaking-changes)

## NuGet packages

- [Pipaslot.Mediator](https://www.nuget.org/packages/Pipaslot.Mediator/) - Core logic for in-process usage
- [Pipaslot.Mediator.Http](https://www.nuget.org/packages/Pipaslot.Mediator.Http/) - Extension for communication across HTTP
