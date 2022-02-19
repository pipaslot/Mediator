## Version 5.0.0
Nuget: **Pipaslot.Mediator**, **Pipaslot.Mediator.Http**

- Mediator server can be configured to provide different status codes (409, 500...) in case of error during processing
- Mediator server accepts HTTP GET requests. See `Pipaslot.Mediator.Http.IMediatorUrlFormatter`. Provides support for file download.

### Fixed
- NotificationProviderMiddlewar: Serving exception even if an exception was thrown during handler processing

### Breaking Changes
- API changed for HttpClientExecutionMiddleware
- API changed for IMediatorMiddleware: method `Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)` was replaced by `Invoke(MediatorContext context, MiddlewareDelegate next)`. Action and cancellation token is available as context property.
- Interface `IPipelineConfigurator` was renamed to `IMediatorConfigurator`
- class `MediatorContext` has private constructor. If you need to create a new instance, you can clone the original context by method `context.CopyEmpty()`

## Version 4.2.0
Nuget: **Pipaslot.Mediator**
 - Added interfaces for sending notifications in action results

## Version 4.1.0
Nuget: **Pipaslot.Mediator**
 - Custom pipeline condition via `.AddPipeline("Condition specified by class reflection", actionType => { ... })`

## Version 4.0.0
Nuget: **Pipaslot.Mediator**, **Pipaslot.Mediator.Http**
 - Client mediator supports its own pipeline, action, and handler registration, but by default, it sends actions to the server via HTTP protocol
 - Serialization logic from server and client was extracted into service IActionSerializer and IResponseSerializer which can be replaced via DI re-configuration
 - New data serializer was implemented supporting full JSON support
 - IMediatorResponse has property Results as object array type providing messages from middlewares
 - All middlewares (except execution middlewares) can have configured ServiceLifetime (default is scoped)
 - ServiceLifetime for handlers can be changed (default is transient)
 - The mediator provides events ActionStarted and ActionCompleted notifying subscribers when a new action is started and completed. These events provide also a collection of all running actions.
 - New method Mediator.ExecuteOrDefault returning data or default object depending on success status
 - Added configurable middleware UseReduceDuplicateProcessing for reducing concurrent action calls to minimize server load 

### Breaking changes
 - Obsolete code from the previous version was removed
 - NuGet packages Pipaslot.Mediator.Client and Pipaslot.Mediator.Server were repalced by Pipaslot.Mediator.Http
 - Service collection method AddMediatorClient returns IPipelineConfigurator instead of IServiceColelction
 - ClientMediator removed was replaced by HttpClientExecutionMiddleware
 - Contracts MediatorRequestSerializable, MediatorResponseDeserialized, MediatorResponseSerializable were moved from Pipaslot.Mediator.Contracts to Pipaslot.Mediator.Http.Contracts
 - Removed support for contract serialization implemented in Version 1
 - Property ServerMediatorOptions.KeepCompatibilityWithVersion1 was removed
 - Class RequestContractExecutor was removed, the behavior was moved to MediatorMiddleware and IContractSerializer
 - Class Pipaslot.Mediator.Server.MediatorExceptionLoggingMiddleware was replaced by Pipaslot.Mediator.Http.Middlewares.ExceptionLoggingMiddleware
 - Class Pipaslot.Mediator.Server.MediatorServerException was replaced by Pipaslot.Mediator.Http.MediatorHttpException
 - Pipeline configuration classes were moved into Pipaslot.Mediator.Configuration

## Version 3.0.0
### Breaking changes
- Pipaslot.Mediator.Client 2.0.0 is not fully compatible with Pipaslot.Mediator.Server 3.0.0
- The following interfaces were abstracted (You can still use them)
  - IMessage was replaced by IMediatorAction
  - IRequest<T> was replaced by IMediatorAction<T>
  - IMessageHandler was replaced by IMediatorHandler
  - IRequestHandler<T> was replaced by IMediatorHandler<T>
