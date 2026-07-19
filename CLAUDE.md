# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

Pipaslot.Mediator implements the Mediator pattern with an ASP.NET Core-style middleware pipeline. Its distinguishing feature over other mediator libraries is that the exact same action/handler code can run **in-process** or be transported **over HTTP** (designed for Blazor WASM client/server apps): a client-side `IMediator` sends an action, `Pipaslot.Mediator.Http` serializes/transports it, and the server executes the real handler and returns the result through the same pipeline abstractions.

NuGet packages published from this repo:
- `Pipaslot.Mediator` — core in-process mediator, middleware pipeline, authorization, notifications.
- `Pipaslot.Mediator.Http` — HTTP transport (client `HttpClientExecutionMiddleware` + server `MediatorMiddleware`), JSON contract serialization.

## Build & test commands

```bash
dotnet build Pipaslot.Mediator.slnx             # build everything (multi-targets net6.0–net10.0 for the core/Http libs)
dotnet test tests/Pipaslot.Mediator.Tests                       # core library tests (xUnit + Moq)
dotnet test tests/Pipaslot.Mediator.Http.Tests                   # HTTP transport / serialization tests
dotnet test --filter "FullyQualifiedName~RuleSet_OperatorTests"  # run a single test class
dotnet test --filter "DisplayName~SomeTestMethodName"            # run a single test method
```

- `tests/Pipaslot.Mediator.Tests` references two helper projects that only exist to be scanned by reflection: `Pipaslot.Mediator.Tests.ValidActions` (well-formed actions/handlers) and `Pipaslot.Mediator.Tests.InvalidActions` (actions intentionally missing a handler). Don't "fix" the invalid-actions project — its lack of handlers is the test fixture.
- `Demo/` (Server + Client Blazor WASM + Shared) is a runnable example app, not a test suite. Use it as a reference for real usage patterns (auth, file upload, notifications, custom middlewares) rather than editing it to satisfy internal library changes.
- `Pipaslot.Mediator.Benchmarks` uses BenchmarkDotNet; results are checked into `Report/results/*.md`. Only re-run/regenerate these when explicitly asked.

## Architecture

### The pipeline model

Every call into the mediator flows through `Mediator.Dispatch`/`Execute` (`Pipaslot.Mediator/Mediator.cs`), which builds a per-action pipeline of `IMediatorMiddleware` and runs it as a chain of `Next` delegates (`MiddlewareDelegate`) — same shape as ASP.NET Core's `RequestDelegate` pipeline. Key pieces:

- **`IMediatorAction` / `IMediatorAction<TResult>`** (`Abstractions/`) are the true root markers the pipeline dispatches on. `IMessage` (no result) and `IRequest<TResponse>` (with result) are the public-facing marker interfaces apps implement on their action DTOs.
- **`IMediatorHandler<TAction>` / `IMediatorHandler<TAction, TResult>`** is what a handler implements for a given action type. Registered via `MediatorConfigurator.AddHandlers*` and resolved by DI at pipeline execution time.
- **`MediatorConfigurator`** (`Configuration/`) is the single place middleware pipelines are assembled: global middlewares via `Use<T>`/`UseWhen`, and fully separate named pipelines via `AddPipeline(condition, ...)` for a subset of action types (only one pipeline may match a given action — `MediatorException.TooManyPipelines` otherwise).
- **`HandlerExecutionMiddleware`** (implements `IExecutionMiddleware`) is always the terminal middleware unless a custom `IExecutionMiddleware` is registered instead — this is how `Pipaslot.Mediator.Http`'s client swaps handler execution for an HTTP call (`HttpClientExecutionMiddleware` is registered as the `IExecutionMiddleware`, see `AddMediatorClient`).
- **`MediatorContext`** carries the action, accumulated `Results`, `ExecutionStatus`, and an `IFeatureCollection` (ASP.NET Core-style extensible per-request feature bag, see `Middlewares/Features/`) through the whole pipeline.
- Nested mediator calls (a handler calling `IMediator` again for another action) get an extra `NotificationPropagationMiddleware` automatically inserted so results/notifications bubble back to the parent context — see the "Nested calls" sequence diagram in `docs/wiki/5.-Mediator-API.md` and `MediatorContextAccessor.Push`.

### Registration entry points

- `services.AddMediator(...)` (`Pipaslot.Mediator/ServiceCollectionExtensions.cs`) — pure in-process setup, returns `IMediatorConfigurator` for further `.Use()`/`.AddPipeline()`/`.AddHandlersFromAssembly()` calls.
- `services.AddMediatorServer(...)` / `services.AddMediatorClient(...)` (`Pipaslot.Mediator.Http/ServiceCollectionExtensions.cs`) — HTTP-flavored setup layered on top of `AddMediator`, adding contract serialization, credible-type providers (anti-deserialization-attack allowlists, see `Configuration/ICredibleProvider.cs`), and the URL formatter used to keep client/server action routing in sync.
- On the server, incoming HTTP requests are intercepted by `MediatorMiddleware` (ASP.NET Core middleware, registered via `app.UseMediator()` from `ApplicationBuilderExtensions`) at a single configurable endpoint (`ServerMediatorOptions.Endpoint`), which deserializes the action, dispatches it through the normal `IMediator`, and writes back the serialized `IMediatorResponse`.

### Authorization (`Pipaslot.Mediator/Authorization/`)

A declarative rule/policy engine independent of ASP.NET Core's authorization system, evaluated by `AuthorizationMiddleware` inside the pipeline (registered automatically) before the handler runs:
- `IPolicy` → `Policy` (AND/OR combinator, supports `&`/`|` operators) and `IdentityPolicy` are composed by handlers implementing `IHandlerAuthorization`/`IHandlerAuthorizationAsync`.
- `Rule`/`RuleSet`/`RuleOutcome` capture the resolved pass/fail tree, which `Authorization/Formatting/` (`INodeFormatter`, `DefaultNodeFormatter`) can render into a human-readable explanation — useful for surfacing *why* an authorization check failed.
- Attributes (`AnonymousPolicyAttribute`, `AuthenticatedPolicyAttribute`, `RolePolicyAttribute`) provide the common cases without hand-writing an `IHandlerAuthorization`.

### Notifications (`Pipaslot.Mediator/Notifications/`)

A pub/sub side channel layered on the same pipeline: handlers can raise `Notification`s that propagate up through nested mediator calls (`NotificationPropagationMiddleware`) to an `INotificationReceiver`/`INotificationProvider` that outer code (e.g., a Blazor component subscribing via `IMediatorContextAccessor`) can observe without changing the handler's return type.

### Serialization (`Pipaslot.Mediator.Http/Serialization/`)

`Serialization/V3/JsonContractSerializer` is the current (System.Text.Json-based) wire format; converters under `V3/Converters/` handle polymorphic interface-typed properties (`InterfaceConverter`) and binary stream payloads embedded in JSON responses (`StreamExtractingConverter`, used for file download/upload scenarios in the Demo). Do not confuse this with the credible-type allowlist (`ICredibleProvider`) — serialization shape and deserialization *trust* are separate concerns.

### Multi-targeting

`Pipaslot.Mediator` and `Pipaslot.Mediator.Http` multi-target `net6.0` through `net10.0` (see their `.csproj`) because they're published to NuGet for consumers on older TFMs; test/benchmark/demo projects target `net10.0` only. When editing the core libraries, keep API usage compatible across that whole range (per-TFM `PackageReference` blocks already pin `Microsoft.Extensions.DependencyInjection.Abstractions` to the matching version).

## Documentation

- Pipeline/call-flow diagrams (component views, in-process calls, HTTP calls, nested calls, custom middleware ordering) live directly in `docs/wiki/2.-Core-concepts-and-glossary.md`, `5.-Mediator-API.md`, `6.-Pipelines-and-Middlewares.md`, and `8.-HTTP-transport-and-configuration-for-Client-Server-usage.md`. Update the relevant diagram there if you change pipeline ordering or the client/server call flow.
- `docs/archive/version4/`, `docs/archive/version5/` — old wiki snapshots, kept for historical reference only; do not treat as current API documentation.
- `docs/wiki/` is the source of truth for the GitHub Wiki: `.github/workflows/wiki-sync.yml` mirrors this folder verbatim (via `rsync --delete`) onto the wiki whenever it changes on `main`, so anything not in `docs/wiki/` will be deleted from the wiki on the next sync. **Whenever a code change affects public API surface, configuration, setup steps, middleware behavior, or any other user-facing behavior described there, update the relevant page(s) under `docs/wiki/` in the same change** — don't leave it for a follow-up. Purely internal refactors with no observable behavior change don't need a wiki update. `Home.md` is the wiki's landing/nav page — add an entry there for any new page.

### Expected wiki page structure

Apply this to every new or edited page under `docs/wiki/`:

- **Footer**: end every page (except `Home.md`, which is itself the nav index, and `Release-notes-and-breaking-changes.md`, a plain changelog) with a `## See also` section listing 1-3 related pages and a short reason each is relevant. Use `## See also` as the only heading name for this — don't reintroduce `## Next steps` or other variants; the wiki previously had both, which is the inconsistency this rule replaces.
- **Glossary links**: the first time a page uses a term defined in `2.-Core-concepts-and-glossary.md` (Mediator, Action, Request, Message, Handler, Pipeline, Middleware, Feature, Context, Response, Result, Facade, ...) and the reader could plausibly land on that page directly (e.g. via search) without having read the glossary first, link it as `[Term](2.-Core-concepts-and-glossary.md#term-anchor)`.
- **No silent duplication**: if a concept is already explained canonically on another page (e.g. `Dispatch`/`Execute`/`DispatchUnhandled`/`ExecuteUnhandled` in `5.-Mediator-API.md`, or a middleware documented in `6.1.-Ready-to-use-middlewares.md`), link to it instead of re-explaining or re-copying it. This includes example/contract code: the Client-Server quickstart (`4.-`) reuses the same `WeatherForecastRequest`/`WeatherForecastResult` shapes as the in-process quickstart (`3.-`) and links back to it instead of re-pasting the code block. Duplicated explanations or code drift apart over time.
- **Anchors**: link to a specific heading with `<page>.md#<slug>`, where `<slug>` follows GitHub's heading-to-slug rule (lowercase, spaces to hyphens, punctuation stripped) — this is the convention already used throughout `docs/wiki/`.
- **Diátaxis separation**: keep Tutorial/How-to content (step-by-step recipes) visually distinguishable from Reference/Explanation content within the same page (e.g. its own heading/subsection) rather than interleaving them under one heading. Where a page legitimately covers more than one Diátaxis category in sequence (e.g. `1.-Why-Pipaslot.Mediator.md` moving from Explanation into a Reference "Library structure" section, or `2.-Core-concepts-and-glossary.md` moving from Explanation into the Reference glossary), add an explicit one-line signpost sentence marking the transition — a heading alone is not enough for the reader to notice the category shift.
- **Historical context belongs in the changelog**: don't narrate past/removed behavior (e.g. "in version X this was done differently") inline in a Reference or How-to chapter. Keep the chapter describing only current behavior, and link to the relevant entry in `Release-notes-and-breaking-changes.md` for readers who want the history (a short "Historical note" subsection with a link is enough if it's worth flagging at all).
- **Explain non-obvious structural choices**: if a Tutorial/How-to page makes a structural choice that isn't self-evident (e.g. splitting a quickstart's code into "Shared" vs "Executable" projects), add a short "why" sentence or paragraph before the steps rather than letting the reader infer it — this applies even though the surrounding content is otherwise pure Tutorial/How-to.
