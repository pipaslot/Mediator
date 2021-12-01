# Release notes and breaking changes

## Version 3
### Breaking changes:
- Pipaslot.Mediator.Client 2.0.0 is not fully compatible with Pipaslot.Mediator.Server 3.0.0
- Interface IMessage was replacedd by IMediatorAction
- Interface IRequest<T> was replacedd by IMediatorAction<T>
- Interface IMessageHandler was replacedd by IMediatornHandler
- Interface IRequestHandler<T> was replacedd by IMediatorHandler<T>

## Version 4
### News:
 - Client mediator supports own pipeline, action and handler registration, but by default it sends actions to server via HTTP protocol
 - Serialization logic from server and client was extracted into service IContracSerializer which can be replaced via DI re-configuration
 - New data serializer was implemented supporting full JSON support
 - IMediatorResponse has property Results as object array type providing messages from middlewares
 - All middlewares (except execution middlewares) can have configured ServiceLifetime (default is scoped)
 - ServiceLifetime for handlers can be changed (default is transient)
 - Mediator provides events ActionStarted and ActionCompleted notifying subscribers when new action is started and completed. These events provides also collection of all running actions.
 - new method Mediator.ExecuteOrDefault returning data or default object depending on succeess status
 - added configurable middleware UseReduceDuplicateProcessing for reducing concurrent action calls to minimize server load 

### Breaking changes:
 - Obsolete code from previous version was removed
 - Nuget packages Pipaslot.Mediator.Client and Pipaslot.Mediator.Server were repalced by Pipaslot.Mediator.Http
 - Service collection method AddMediatorClient returns IPipelineConfigurator instead of IServiceColelction
 - ClientMediator removed was replaced by HttpClientExecutionMiddleware
 - Contracts MediatorRequestSerializable, MediatorResponseDeserialized, MediatorResponseSerializable were moved from Pipaslot.Mediator.Contracts to Pipaslot.Mediator.Http.Contracts
 - Removed support for contract serialization implemented in Version 1
 - property ServerMediatorOptions.KeepCompatibilityWithVersion1 was removed
 - plass RequestContractExecutor was removed, behavior was moved to MediatorMiddleware and IContractSerializer
 - plass Pipaslot.Mediator.Server.MediatorExceptionLoggingMiddleware was replaced by Pipaslot.Mediator.Http.Middlewares.ExceptionLoggingMiddleware
 - Class Pipaslot.Mediator.Server.MediatorServerException was replaced by Pipaslot.Mediator.Http.MediatorHttpException
 - Pipelineconfiguration classes were moved into Pipaslot.Mediator.Configuration
