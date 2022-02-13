# Motivation
This library was built to achieve the following goals:
- Decouple service references (define actions and handlers).
- Monitor activities executed through the mediator (via middlewares)
- Provide pre-processing and post-processing rules (via middlewares)
- Use CQRS principles: Segregate responsibilities for Requests/Queries and Messages/Commands with own monitoring, post-processing, and pre-processing rules (via pipelines)
- Ability to define custom action types like Requests or Messages with their own pipelines 
- Use shared library containing 
- Build simple communication protocol for Blazor WASM application without duty to write and maintain API
- Strongly typed and object-oriented communication across HTTP
- Share data contracts between client and server to avoid duplicate definitions and maintenance API of contracts
- Provide interface with unified behavior on the client and as well on the server-side
- Ability to call another action from handlers or middlewares and their chaining. (We recommend using it wisely)
- Executing operations as async but with the ability to await for at least status type or for the result
- Provide result collection in mediator response to provide additional data from middleware execution (Usefull for user notifications or validation errors processed by the client as side effects)

# Library structure
This library was since version 4 split into two NuGet packages.
**Pipaslot.Mediator** 
- Core logic and interfaces defining mediator as service, actions, handlers, middlewares 
- `IServiceCollection` extension `.AddMediator()` providing registration for actions and handlers and pipeline configurations

**Pipaslot.Mediator.Http**
- Services and logic for serialization and transferring via HTTP 
- Provide mediator overload `.AddMediatorClient()` which sends actions to server endpoint by default instead of searching for handlers to execute
- Provide mediator overload `.AddMediatorServer()` which converts HTTP content back to mediator action execute Server-side mediator and handler execution
- `IApplicationBuilder` extension `.UseMediator() registering mediator as middleware for ASP.NET Core request pipeline

# Glossary
- **Mediator** - Service represented by interface `IMediator` executing Actions and providing a response with status and results.
- **Action** - Top-level abstraction for data incoming into the mediator and describing expected results after successful processing. We can logically split the actions into two concepts with more specific names _Request_ and _Message_. We can define our own custom action types and name them by our preferences, but they will have to use either concept of Request or Message
- **Message** - Implementation of `IMessage` or `IMediatorAction` action type processed by a handler where no result is expected in the mediator response. The mediator will provide an only status whether the handler was processed successfully.
- **Request** - Implementation of `IRequest<TResult>` or `IMediatorAction<TResult>` action type processed by a handler where the result is expected after successful processing.
- **Response** - Result wrapper providing status whether the processing was successful, Result for Request and Results from middlewares
- **Result collection** - Collection of objects attached by middlewares or returned from handlers. The mediator supports to specify in the pipeline that actions can be handled by multiple handlers. In that case, all handler execution results will be added to this collection. 
- **Result** - Single structure is only taken from Result collection with type matching `TResult` type defined in `IRequest<TResult>` or `IMediatorAction<TResult>`. Returns the first occurrence if multiple objects of the same type are in the Result collection.
- **Handler** - Top-level abstraction for action execution. The handlers can de be divided by their concept to Message handlers and Request handlers.
- **Message handler** - Implementation of`IMessageHandler<in TRequest>` or `IMediatorHandler<TRequest>` executing message without result provided by method `Handle`
- **Request handler** - Implementation of`IRequestHandler<in TRequest, TResponse>` or `IMediatorHandler<TRequest, TResponse>` executing request and providing result by method `Handle`
- **Pipeline** - Collection of one or more middlewares specified for one Action implementing specific interface like `IRequest`, `IMessage` or by your own action types
- **Default pipeline** - Default pipeline applied to actions without their own specific pipelines
- **Middleware** - Class wrapping handler executions similarly to ASP.NET Core middleware wrapping request execution. Provide the ability to define multiple pre-processing and post-processing actions applicable for multiple pipelines. Middleware is not Action specific.
- **Context** - Data structure handed over across middlewares aggregating results in Result collection and all errors occurred or handled in the pipeline.

# Scope of mediator usage

## In-process only
NuGet package `Pipaslot.Mediator` can be used internally by applications without the need to communicate over HTTP. The action execution and handling are described below:

![Use of mediator in-process](../img/mediator-in-process.png)

In the easiest scenario, you can use a mediator without pipeline specification. In that case, the default pipeline will be applied containing only the middleware executing handler.

## Over Http
By registering mediator via `.AddMediatorClient()` on client-side and `.AddMediatorServer()` on server-side. All actions will be automatically transferred from client to server where are handlers defined. You can specify pipelines for the client as well as for the server which is useful for example for global error handling end-user notifications.
![Use of mediator over HTTP](../img/mediator-over-http.png)