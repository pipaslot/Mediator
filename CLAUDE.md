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
dotnet test tests/Pipaslot.Mediator.Tests                       # core library tests (xUnit + NSubstitute)
dotnet test tests/Pipaslot.Mediator.Http.Tests                   # HTTP transport / serialization tests
dotnet test --filter "FullyQualifiedName~RuleSet_OperatorTests"  # run a single test class
dotnet test --filter "DisplayName~SomeTestMethodName"            # run a single test method
```

- `tests/Pipaslot.Mediator.Tests/ValidActions/` holds well-formed action/handler fixtures shared by both test projects (`Pipaslot.Mediator.Http.Tests` reaches it via its `ProjectReference` to `Pipaslot.Mediator.Tests`). It's a folder, not a separate assembly, so `AddActionsFromAssemblyOf<T>()`/`AddHandlersFromAssemblyOf<T>()` on any type in it now scans the **whole** `Pipaslot.Mediator.Tests` assembly, not just this folder — a test that needs exactly (and only) these fixtures registered must use explicit `AddActions([...])`/`AddHandlers([...])` instead (see `HandlerExistenceCheckerTests.Verify_RegisteredAssemblyWithValidActions_DoesNotThrowExceptions`), or it'll incidentally pick up unrelated handler fixtures from elsewhere in the project that aren't constructible outside their own test's manual DI wiring.
- `tests/Pipaslot.Mediator.Tests.InvalidActions` stays a **separate project**, this one's value depends on being a real, physically separate assembly that is guaranteed to contain zero handlers (see `HandlerExistenceCheckerTests`, which does a real `AddHandlersFromAssemblyOf<T>()` scan against it to verify "no handler found" detection). Don't "fix" it by adding a handler.
- Both xUnit test projects need `xunit.runner.visualstudio` to be discoverable by `dotnet test` (plain VSTest CLI, unlike Rider's own test runner, needs this adapter explicitly — it's not pulled in transitively by `Microsoft.NET.Test.Sdk`/`xunit`). If `dotnet test` ever reports "A total of 1 test files matched the specified pattern." followed by zero tests run instead of a pass/fail summary, this package reference is what's missing from the `.csproj`.
- `Demo/` (Server + Client Blazor WASM + Shared) is a runnable example app, not a test suite. Use it as a reference for real usage patterns (auth, file upload, notifications, custom middlewares) rather than editing it to satisfy internal library changes.
- `Pipaslot.Mediator.Benchmarks` uses BenchmarkDotNet; results are checked into `Report/results/*.md`. Only re-run/regenerate these when explicitly asked.

### Code coverage

Both test projects already reference `coverlet.collector` (in-box, no install needed). To produce an HTML coverage report for `Pipaslot.Mediator` and `Pipaslot.Mediator.Http`:

```bash
dotnet test tests/Pipaslot.Mediator.Tests --collect:"XPlat Code Coverage" --results-directory TestResults/Mediator
dotnet test tests/Pipaslot.Mediator.Http.Tests --collect:"XPlat Code Coverage" --results-directory TestResults/Http

dotnet tool install -g dotnet-reportgenerator-globaltool   # one-time, skip if already installed
reportgenerator -reports:"TestResults/Mediator/**/coverage.cobertura.xml;TestResults/Http/**/coverage.cobertura.xml" -targetdir:TestResults/CoverageReport -reporttypes:Html
```

Open `TestResults/CoverageReport/index.html`. `TestResults/` is a generated artifact directory, not checked in — re-run the commands above rather than expecting a stale report to exist.

### Test-writing conventions

- **Pair every one-directional wire-format test with a round-trip test.** A test that only serializes and asserts an exact JSON string (or only deserializes a hand-written JSON string) verifies just that one direction — it gives no signal about whether the other direction (`Read` vs `Write`) still works for that same payload shape. Keep exact-string assertions where the wire format itself is the contract under test, but add a companion round-trip test for the same shape whenever one doesn't already exist.
- **Check for existing coverage before adding a test.** Test classes are organized by scenario/trigger condition, not by which unit of work introduced the assertion — a class whose doc comment says "new API only" or "not covered elsewhere" is a claim about the state *when that comment was written*, not a guarantee. Before adding a new test for behavior that might already be exercised, search for an existing test with the same setup/trigger and extend it instead of adding a parallel one elsewhere; two tests asserting the same trigger condition from two different files is a sign the search was skipped.
- **`Pipaslot.Mediator.Http.Tests` reuses `Pipaslot.Mediator.Tests`'s `Factory` for setup that isn't Http-specific** (currently just `Factory.CreateServiceProvider`), via a `ProjectReference` plus `InternalsVisibleTo` on `Pipaslot.Mediator.Tests.csproj`, rather than a separate `Tests.Common` project. Add new shared test setup helpers to `Pipaslot.Mediator.Tests`'s `Factory` and call them from Http's `Factory` (see `CreateMediator`) instead of duplicating them — reserve a `Tests.Common` project for if the shared surface grows enough that a one-way dependency between the two test projects stops making sense.
- **`Factory` never registers actions/handlers by assembly-wide scan** (no `AddActionsFromAssembly(Assembly)`/`AddHandlersFromAssembly(Assembly)` self-scan). Every test passes `Factory.CreateCustomMediator`/`CreateConfiguredMediatorWithLogger` a `setup` that explicitly lists exactly the handler types (and, for HTTP/`ReflectionCache`-relevant tests, action types) it dispatches to — nothing more. Before registering a handler for an action, check whether the middleware in front of it actually calls `next(context)` — a middleware that doesn't (a terminal validator/blocker fixture) means the handler is never reached, and it should not be registered at all.
- **One assertion per test where a failure has a distinct meaning; multiple assertions where they form one indivisible contract.** E.g. three separate tests each asserting one property of a success result are fine if any one of them could fail for an unrelated reason; asserting `Success` and `GetErrorMessage()` together is fine when they're two facets of the same failure outcome being verified at once.
- **Separate Arrange/Act/Assert with a blank line** in new or edited tests. Don't do a dedicated pass over old tests just to add the blank lines — unify incrementally, whenever you're already editing a test for another reason.

### Where a test belongs

Three levels, one mechanical rule. Grep the test for `Dispatch(` / `Execute(`:

| Level | Rule | Folder | Class name |
|---|---|---|---|
| Unit | SUT is one production type; no DI container | mirrors the production namespace | `<Type>Tests` / `<Type>_<Aspect>Tests` |
| Wiring | Builds a container but never dispatches; SUT is the configuration result (pipeline shape, registration errors) | mirrors the production namespace of the wiring type (`Configuration/`, `Services/`) | `<Type>_<Aspect>Tests` |
| E2E | Calls `IMediator.Dispatch`/`Execute`/`*Unhandled` on a real container | `E2E/<theme>/` | `<Scenario>Tests` |

- **Every test class ends with `Tests`.** The file name and the class name must match exactly.
- **Test methods are named `Method_Condition_ExpectedOutcome`.**
- **Nested fixtures inside a test class are `private`.** The moment a second class needs one, move it to
  `E2E/Fixtures/` — never widen the visibility of a nested class to share it. A test that reaches into another
  test class's nested type couples two files that look independent.
- **`Tests.InvalidActions` holds only actions without handlers.** Middlewares and helpers belong to the test
  project that uses them.
- **Assert the exception type plus its key data, not the whole formatted message.** Comparing against
  `SomeException.Create(...).Message` makes the test reimplement the production code it is checking.
- **A test class doc comment states the trigger condition and how it differs from neighbouring test classes.**
  It must not narrate history ("before unit 4…", "introduced in version X") — that belongs in
  `docs/wiki/Release-notes-and-breaking-changes.md`.
- **Static state in `Tests.ValidActions` fixtures is reset in the test class constructor**, never inline inside a
  test method. Classes sharing the same static fixture must share an xUnit `[Collection]`.

## Architecture

### The pipeline model

Every call into the mediator flows through `Mediator.Dispatch`/`Execute` (`Pipaslot.Mediator/Mediator.cs`), which builds a per-action pipeline of `IMediatorMiddleware` and runs it as a chain of `Next` delegates (`MiddlewareDelegate`) — same shape as ASP.NET Core's `RequestDelegate` pipeline. Key pieces:

- **`IMediatorAction` / `IMediatorAction<TResult>`** (`Abstractions/`) are the true root markers the pipeline dispatches on. `IMessage` (no result) and `IRequest<TResponse>` (with result) are the public-facing marker interfaces apps implement on their action DTOs.
- **`IMediatorHandler<TAction>` / `IMediatorHandler<TAction, TResult>`** is what a handler implements for a given action type. Registered via `MediatorConfigurator.AddHandlers*` and resolved by DI at pipeline execution time.
- **`MediatorConfigurator`** (`Configuration/`) is the single place middleware pipelines are assembled: global middlewares via `Use<T>`/`UseWhen`, and fully separate named pipelines via `AddPipeline(condition, ...)` for a subset of action types (only one pipeline may match a given action — `MediatorException.TooManyPipelines` otherwise).
- **`HandlerExecutionMiddleware`** (implements `IExecutionMiddleware`) is always the terminal middleware unless a custom `IExecutionMiddleware` is registered instead — this is how `Pipaslot.Mediator.Http`'s client swaps handler execution for an HTTP call (`HttpClientExecutionMiddleware` is registered as the `IExecutionMiddleware`, see `AddMediatorClient`).
- **`MediatorContext`** carries the action, accumulated `Results`, `ExecutionStatus`, and an `IFeatureCollection` (ASP.NET Core-style extensible per-request feature bag, see `Middlewares/Features/`) through the whole pipeline.
- Nested mediator calls (a handler calling `IMediator` again for another action) get an extra `NotificationPropagationMiddleware` automatically inserted so results/notifications bubble back to the parent context — see the "Nested calls" sequence diagram in `docs/wiki/5.-Mediator-API.md` and `MediatorContextAccessor.Push`.
- **Design principle: nested calls must not touch the shared `HttpContext`.** Only the root context (applied centrally from `Pipaslot.Mediator.Http.MediatorMiddleware` after the pipeline finishes) may write to `HttpContext`

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

Because tests only run on `net10.0`, a `#if <TFM>` conditional compilation block (e.g. `MediatorContext.CreateGuid`'s `#if NET9_0_OR_GREATER`) only gets test coverage for whichever branch `net10.0` selects — the other branch compiles but is never exercised by `dotnet test`. This is a deliberate CI-time tradeoff (multi-targeting the test projects would multiply CI duration ~5x), not an oversight. When adding a new `#if TFM` block, prefer branches that only swap the underlying API call for an equivalent result (as `CreateGuid` does) over branches with diverging behavior — diverging behavior in an untested branch is a correctness risk this setup can't catch.

## Code comments

- **Never cite a planning/analysis document (e.g. anything under `docs/Todos/`) in a code comment, XML doc, or commit-adjacent annotation.** Those documents are working artifacts for a single change; once the change ships they stop being maintained and often get deleted, leaving a comment that points at a file which no longer exists or no longer reflects reality. `docs/wiki/` is the only doc tree that's a maintained, permanent source of truth — linking to a wiki page (e.g. `see docs/wiki/6.2.-Exception-handling.md`) is fine.
- Instead, write the comment so it stands on its own: state the business rule, invariant, or constraint the code enforces, not which document or unit of work introduced it. E.g. prefer "does not create a `Notification` — recorded exceptions must not leak into client-facing `Results`" over "see 1.3 Unit 3 — per the design doc, `AddException` must not touch `Results`".

## Documentation

- Pipeline/call-flow diagrams (component views, in-process calls, HTTP calls, nested calls, custom middleware ordering) live directly in `docs/wiki/2.-Core-concepts-and-glossary.md`, `5.-Mediator-API.md`, `6.-Pipelines-and-Middlewares.md`, and `8.-HTTP-transport-and-configuration-for-Client-Server-usage.md`. Update the relevant diagram there if you change pipeline ordering or the client/server call flow.
- `docs/archive/version4/`, `docs/archive/version5/` — old wiki snapshots, kept for historical reference only; do not treat as current API documentation.
- `docs/wiki/` is the source of truth for the GitHub Wiki: `.github/workflows/wiki-sync.yml` mirrors this folder verbatim (via `rsync --delete`) onto the wiki whenever it changes on `main`, so anything not in `docs/wiki/` will be deleted from the wiki on the next sync. **Whenever a code change affects public API surface, configuration, setup steps, middleware behavior, or any other user-facing behavior described there, update the relevant page(s) under `docs/wiki/` in the same change** — don't leave it for a follow-up. Purely internal refactors with no observable behavior change don't need a wiki update. `Home.md` is the wiki's landing/nav page — add an entry there for any new page.
- **Every change with user-observable behavior also gets a changelog bullet.** Add it under `## Unreleased` at the top of `docs/wiki/Release-notes-and-breaking-changes.md`, in the same change — don't leave it for a follow-up, and don't invent a version number (that's assigned later, at release time; see `CONTRIBUTING.md`). Purely internal refactors with no observable behavior change don't need an entry.
- **Changelog bullets are one terse sentence each** — what changed, not why or how. No rationale, no design justification, no enumeration of edge cases/exceptions/behavioral nuances. If a reader needs more than the one-liner, link to the relevant `docs/wiki/` page (`[Page](page.md#anchor)`) rather than explaining inline — the wiki page is where that detail belongs and stays current.

### Expected wiki page structure

Apply this to every new or edited page under `docs/wiki/`:

- **Footer**: end every page (except `Home.md`, which is itself the nav index, and `Release-notes-and-breaking-changes.md`, a plain changelog) with a `## See also` section listing 1-3 related pages and a short reason each is relevant. Use `## See also` as the only heading name for this — don't reintroduce `## Next steps` or other variants; the wiki previously had both, which is the inconsistency this rule replaces.
- **Glossary links**: the first time a page uses a term defined in `2.-Core-concepts-and-glossary.md` (Mediator, Action, Request, Message, Handler, Pipeline, Middleware, Feature, Context, Response, Result, Facade, ...) and the reader could plausibly land on that page directly (e.g. via search) without having read the glossary first, link it as `[Term](2.-Core-concepts-and-glossary.md#term-anchor)`.
- **No silent duplication**: if a concept is already explained canonically on another page (e.g. `Dispatch`/`Execute`/`DispatchUnhandled`/`ExecuteUnhandled` in `5.-Mediator-API.md`, or a middleware documented in `6.1.-Ready-to-use-middlewares.md`), link to it instead of re-explaining or re-copying it. This includes example/contract code: the Client-Server quickstart (`4.-`) reuses the same `WeatherForecastRequest`/`WeatherForecastResult` shapes as the in-process quickstart (`3.-`) and links back to it instead of re-pasting the code block. Duplicated explanations or code drift apart over time.
- **Anchors**: link to a specific heading with `<page>.md#<slug>`, where `<slug>` follows GitHub's heading-to-slug rule (lowercase, spaces to hyphens, punctuation stripped) — this is the convention already used throughout `docs/wiki/`.
- **Diátaxis separation**: keep Tutorial/How-to content (step-by-step recipes) visually distinguishable from Reference/Explanation content within the same page (e.g. its own heading/subsection) rather than interleaving them under one heading. Where a page legitimately covers more than one Diátaxis category in sequence (e.g. `1.-Why-Pipaslot.Mediator.md` moving from Explanation into a Reference "Library structure" section, or `2.-Core-concepts-and-glossary.md` moving from Explanation into the Reference glossary), add an explicit one-line signpost sentence marking the transition — a heading alone is not enough for the reader to notice the category shift.
- **Historical context belongs in the changelog**: don't narrate past/removed behavior (e.g. "in version X this was done differently") inline in a Reference or How-to chapter. Keep the chapter describing only current behavior, and link to the relevant entry in `Release-notes-and-breaking-changes.md` for readers who want the history (a short "Historical note" subsection with a link is enough if it's worth flagging at all).
- **Explain non-obvious structural choices**: if a Tutorial/How-to page makes a structural choice that isn't self-evident (e.g. splitting a quickstart's code into "Shared" vs "Executable" projects), add a short "why" sentence or paragraph before the steps rather than letting the reader infer it — this applies even though the surrounding content is otherwise pure Tutorial/How-to.
