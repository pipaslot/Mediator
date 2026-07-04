See release changes [here](https://github.com/pipaslot/Mediator/wiki/Release-notes-and-breaking-changes).

## Motivation
The Mediator concept is an alternative to SOA (Service-Oriented Architecture), but this library goes one step further by providing abstraction via HTTP for applications using backend for a frontend pattern. It is best suited for .NET Blazor WASM applications.

### Core Architectural Motivations
* **Decouple service dependencies and orchestration logic**<br>
Replace direct service-to-service calls with actions (requests) and handlers, ensuring each use case is isolated and independently testable.

* **Centralize activity monitoring and control flow**<br>
Use middleware (pipeline behaviours) to uniformly monitor, log, and instrument all operations executed through the mediator.

* **Enable consistent pre- and post-processing**<br>
Apply global behaviours (e.g., validation, authorization, transaction scope, caching, error handling) before and after each handler — without duplicating logic in services.

* **Adopt CQRS principles**<br>
Separate Commands/Messages (state-changing actions) from Queries/Requests (read-only operations).
Each type can have its own middleware stack for dedicated pre/post behaviors and telemetry.

### Integration & Communication Motivations

* **Unify client–server communication (e.g., for Blazor WASM)**<br>
Use mediator actions as a lightweight, strongly typed communication protocol between client and server, avoiding the need to manually define and maintain REST or gRPC APIs.

* **Provide strongly typed, object-oriented interaction across HTTP**<br>
Serialize mediator actions and responses over HTTP, maintaining compile-time type safety, predictable contracts, and IDE-assisted navigation.

* **Share data contracts between client and server**<br>
Use common DTOs and message contracts to eliminate redundant definitions and reduce maintenance overhead in API schema evolution.

* **Expose a unified interface on both client and server**<br>
Keep consistent programming model (e.g., IMediator.Execute() or IMediator.Dispatch()) so client-side calls mirror server-side handler execution behaviour.

### Advanced Behavioral Motivations

* **Enable handler chaining and internal action calls**<br>
Allow one handler or middleware to invoke other mediator actions for complex orchestration.<br>
(Recommended for controlled scenarios — excessive chaining can reduce clarity and traceability.)

* **Support asynchronous execution with flexible awaiting**<br>
Handlers run asynchronously, allowing callers to await either the operation’s result or just a completion status, depending on use-case semantics.

* **Return rich response metadata**<br>
Include a result envelope that carries additional middleware-generated data (e.g., validation summaries, user notifications, domain events) — enabling the client to react to side effects beyond the main result.

### Authorization and Access Control

* **Centralized enforcement of authorization rules**<br>
Mediator provides a single entry point for executing business actions, which makes it ideal for consistent and centralized access control.
Instead of scattering [Authorize] attributes or role checks across multiple controllers or services, authorization logic can be enforced at one level — within middleware (pipeline behavior) or directly at the action/handler level.

* **Flexible authorization strategies**<br>
Authorization can be applied:

* > **Globally** – through a middleware that validates every request before reaching its handler (e.g., verifying user identity, roles, claims, or permissions).
* > **Per-action** – by annotating actions or handlers with metadata (e.g., [RolePolicy("Admin")]) and letting the middleware interpret these rules dynamically.
* > **Context-aware** – handlers can still perform fine-grained checks based on domain-specific rules or resource ownership.

* **Unified security model for client and server**<br>
When using Mediator as a communication layer (e.g., in Blazor WebAssembly), the same action that triggers a handler on the server can also carry authorization context (JWT, identity claims).
This maintains a consistent access control model across tiers without duplicating policy definitions.


## Library structure
This library was since version 4 split into two NuGet packages.
**Pipaslot.Mediator** 
- Core logic and interfaces defining mediator as service, actions, handlers, middlewares 
- `IServiceCollection` extension `.AddMediator()` providing registration for actions, handlers and middlewares

**Pipaslot.Mediator.Http**
- Services with logic for serialization and transferring via HTTP 
- Provide mediator overload `.AddMediatorClient()` which sends actions to the server endpoint by default instead of searching for handlers to execute locally
- Provide mediator overload `.AddMediatorServer()` which converts HTTP content back to mediator action executes Server-side mediator and executes handlers
- `IApplicationBuilder` extension `.UseMediator() registering mediator as middleware for ASP.NET Core request pipeline

## Glossary
- **Mediator** - Service represented by interface `IMediator` executing Actions and providing a response with status and results.
- **Action** - Top-level abstraction for data incoming into the mediator and describing expected results after successful processing. We can logically split the actions into two concepts with more specific names _Request_ and _Message_. We can define our own custom action types and name them by our preferences, but they will have to use either the concept of Request or Message
- **Message** - Implementation of `IMessage` or `IMediatorAction` action type processed by a handler where no result is expected in the mediator response. The mediator will provide only the status of whether the handler was processed successfully.
- **Request** - Implementation of `IRequest<TResult>` or `IMediatorAction<TResult>` action type processed by a handler where the result is expected after successful processing.
- **Response** - Result wrapper providing status whether the processing was successful, Result for Request, and Results from middlewares
- **Result collection** - Collection of objects attached by middlewares or returned from handlers. The mediator supports specifying in the pipeline that actions can be handled by multiple handlers. In that case, all handler execution results will be added to this collection. 
- **Result** - Single structure is only taken from Result collection with type matching `TResult` type defined in `IRequest<TResult>` or `IMediatorAction<TResult>`. Returns the first occurrence if multiple objects of the same type are in the Result collection.
- **Handler** - Top-level abstraction for action execution. The handlers can de be divided by their concept into Message handlers and Request handlers.
- **Message handler** - Implementation of`IMessageHandler<in TRequest>` or `IMediatorHandler<TRequest>` executing message without result provided by method `Handle`
- **Request handler** - Implementation of`IRequestHandler<in TRequest, TResponse>` or `IMediatorHandler<TRequest, TResponse>` executing request and providing result by method `Handle`
- **Pipeline** - Middleware collection applied by custom conditions.
- **Middleware** - Class wrapping handler executions similar to ASP.NET Core middleware wrapping request execution. Provide the ability to define multiple pre-processing and post-processing logic. The Middlewares are not Action-specific.
- **Context** - Data structure handed over across middlewares aggregating results in Result collection and all errors occurred or handled in the pipeline.
- **Facade**(IMediatorFacade) - It is a service providing capabilities from all Mediator services. Can be used to access the Context, Notification Provider (appends notification as an independent result), and the Mediator itself.
- **Feature** - Strongly typed structure that may be added to the Context. Used for communication between different middlewares.
