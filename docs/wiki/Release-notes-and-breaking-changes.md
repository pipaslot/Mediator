## Unreleased
* Added `IMediatorExceptionHandler<TException>` with `AddExceptionHandler`/`AddExceptionHandlers` for translating exceptions into client-facing messages — see [6.2.-Exception-handling.md](6.2.-Exception-handling.md).
* Added `MediatorContext.AddException` and `MediatorContext.Exceptions`, letting a middleware fail an action while keeping the original exception server-side — see [6.2.-Exception-handling.md](6.2.-Exception-handling.md#contextaddexception-and-contextexceptions).
* Added `MediatorUnhandledErrorException`, `MediatorNoHandlerFoundException` and `MediatorMissingResultException` as subtypes of `MediatorExecutionException`.
* Added the opt-in `OperationCanceledExceptionHandler`, reporting a cancelled action with a dedicated message and a Warning log entry — see [6.2.-Exception-handling.md](6.2.-Exception-handling.md#report-cancellation-as-something-other-than-a-generic-failure).
* `DispatchUnhandled`/`ExecuteUnhandled` now rethrow exceptions recorded via `AddException` with their original type instead of wrapping them in `MediatorUnhandledErrorException` — see [6.2.-Exception-handling.md](6.2.-Exception-handling.md#contextaddexception-and-contextexceptions).
* `MediatorExecutionException.CreateForUnhandledError` was marked obsolete; use `MediatorUnhandledErrorException.Create` instead.
* Fix: `IHandlerExistenceChecker.Verify` with `CheckExistingPolicies` no longer flags an action as missing authorization when it implements `IActionAuthorization` directly (without also carrying a policy attribute).

### Breaking changes
* `Dispatch`/`Execute` no longer copy a caught exception's `Message` into the response; an exception without a registered handler produces a generic message and an `Error`-level log entry — see [6.2.-Exception-handling.md](6.2.-Exception-handling.md#safe-by-default).
* A middleware catching exceptions to convert them into error messages should be replaced by typed exception handlers — see [Migrating from a catch-all ErrorHandlingMiddleware](6.2.-Exception-handling.md#migrating-from-a-catch-all-errorhandlingmiddleware).
* Adding an error-typed `Notification` via `MediatorContext.AddResult`/`INotificationProvider.Add` (and the `AddError`/`AddWarning`/etc. helpers built on it) no longer sets `MediatorContext.Status` to `Failed` by itself — only `MediatorContext.AddError()`/`AddErrors()` do that now. See [Notifications in server response](9.4.-Cookbook-and-integrations.md#notifications-in-server-response).

## Version 8.5.0
* Added `Pipaslot.Mediator.Http.IMediatorHttpResult`, letting a handler return a result applied directly to the HTTP response — see [9.3.-Custom-HTTP-responses-and-file-download.md](9.3.-Custom-HTTP-responses-and-file-download.md). Additive, non-breaking.
* Added `Pipaslot.Mediator.Http.MediatorContextExtensions.SetResponseStatusCodeHint`, letting a middleware or handler hint the root HTTP response status code without touching `HttpContext` directly — see [8.-HTTP-transport-and-configuration-for-Client-Server-usage.md](8.-HTTP-transport-and-configuration-for-Client-Server-usage.md#server-choosing-the-http-status-code). Additive, non-breaking.
* Fix: a nested action's error notification, once forwarded into an ancestor context, no longer flips that ancestor's own ExecutionStatus/Success — see [Notification propagation across nested calls](9.4.-Cookbook-and-integrations.md#notification-propagation-across-nested-calls).

## Version 8.4.0
* Added `MediatorContext.Depth` and `MediatorContext.IsNested` to expose the nesting level of the current execution (1 = root execution, `IsNested` true when `Depth > 1`).

## Version 8.3.1
* Removed the unnecessary System.Text.Json dependency to simplify the codebase.
* Added support for serialization of Rule and RuleSet objects.
* Made the default Rule object read-only.
* Added an implicit conversion from Rule to RuleSet
* ReduceDuplicateProcessingMiddleware was marked as obsolete

## Version 8.3.0
* Upgrade to .NET 10
* MediatorContext uses Sequential GUID Version 7
* Fix: Register service `IMediatorFacade` only when ContextAccessor is available (not disabled) in the mediator setup

## Version 8.2.2
- MediatorMiddleware: Prevent serializing result when response already started (custom result writes)

## Version 8.2.1 Server-side pre-rendering optimizations
- Added IMediatorUrlFormatter for Server side
- Added INotificationReceiver for Server side

## Version 8.2.0
- Performance and memory consumption improvements (hot paths and for serializer)
- IMediatorContextAccessor was made an optional service, as a performance optimization for cases where it is not needed
- Serializer: Added support for DateOnly and TimeOnly types

## Version 8.1.0
- Performance and memory consumption improvements

## Version 8.0.1
 - Improved detection of HTTP request for UseDirectHttpCallProtectionMiddleware

## Version 8.0.0
- An extra layer was added when converting auth Rules to user-friendly text.
Rules and their combinations in RuleSets are converted to Nodes with evaluated access grants (RuleOutcome).
Nodes are then pruned to eliminate nodes without any effect on the final grant (RuleOutcome), and then they are formatted.
- Added support for nullable Action result types
- Added support for streams and streaming from Client to server (streams are extracted during serialization and body is changed to MultipartFormData instead of JSON)

### Breaking changes
- Support for .netstandard2.0 was dropped
- IRule, IRuleFormatter, EvaluatedRule, and IEvaluatedRule - were removed
- DefaultRuleFormatter - was replaced by DefaultNodeFormatter and is used as template
- Serializer V2 was removed (including serializer type selection)
- Default Error HTTP status code was set to 500 (previously 200 was used)

## Version 7.6.5 - Mediator.Http
- MediatorMiddleware: Prevent serializing result when response already started (custom result writes)

## Version 7.6.4
 - Improved detection of HTTP request for UseDirectHttpCallProtectionMiddleware

## Version 7.6.3
- AuthorizeRequest: fixed serialization

## Version 7.6.2 
- ExceptionLoggingMiddleware: Prevent dumping action body when logging errors

## Version 7.6.0
- Added .NET 8 support

## Version 7.5.0
- Performance optimizations with ConfigureAwait(false)
- Added `MediatorContext.Features` for sharing data between middleware
- Added `IMediatorRegistrator.UseWithParameters` and new overloads for `IMediatorRegistrator.Use` methods to pass custom parameters handled by the middleware. The parameters are available as `MiddlewareParametersFeature`
- Added notification propagation in case of nested mediator calls. Class `Notification` has a new property `StopPropagation` preventing the default propagation.

## Version 7.4.0
- Middleware registration was extended with the method `UseWhenActions` executing the middleware if any MarkerType was implemented

## Version 7.3.0
- Middleware registration was extended with methods `UseWhenNotAction` and `UseWhenNotDirectHttpCall`

## Version 7.2.0
- Added ISingleton and IScoped handler interface to change the default Transient ServiceLifetime

## Version 7.1.0
- Added a new overload `Pipaslot.Mediator.Configuration.MediatorConfigurator.UseWhen((IMediatorAction, IServiceProvider) => bool)` supporting application service resolving.
- Added new overloads for `Pipaslot.Mediator.Configuration.MediatorConfigurator.AddPipelineForActions` specifying multiple action types for what the middlewares will be applied
- Added methods `UseWhenDirectHttpCall` and `UseAuthorizationWhenDirectHttpCall` on `Pipaslot.Mediator.Configuration.MediatorConfigurator` applying the specified middleware only on the first call from HTTP

## Version 7.0.0
- Added an action extension method for formatting action names to friendly names `Pipaslot.Mediator.Abstractions.MediatorActionExtensions.GetActionFriendlyName`
- Added temporary workaround on `MediatorContext.IgnoreActionErrors` for ignoring ActionError notification types
- Allow policy attributes to be defined on interfaces 

### Breaking changes
- RuleSetReproducibility was replaced by RuleScope
- Authorization: Grant calculation was replaced by RuleOutcome and AccessType (RuleSet API was changed)
- Action and handler `IsAuthorizedRequest` was renamed to `AuthorizeRequest`
- `IHandlerExistenceChecker.Verify` signature was changed.
- NotificationType.ActionError was removed and the exception caught during handler execution won't be sent as a notification.
- Obsolete properties `IMediatorResponse.ErrorMessage` and `IMediatorResponse.ErrorMessages` were removed.
- Serializer V3 is used as the default

## Version 6.3.0
Nuget: **Pipaslot.Mediator**, **Pipaslot.Mediator.Http**
- Added .NET 7 target framework
- Added `IMediatorFacade` combining services `IMediator`, `IMediatorContextAccessor`, and `INotificationProvider`
- Fix: Propagate notifications from a nested handler to all parent results

## Version 6.2.0
Nuget: **Pipaslot.Mediator**, **Pipaslot.Mediator.Http**
- Added support for defining actions without handlers (to suppress exceptions from HandlerExistenceChecker, add `NoHandlerAttribute` to the action class).
- Added authorization support. For more details see [documentation](https://github.com/pipaslot/Mediator/wiki/7.-Authorization).

## Version 6.1.0
Nuget: **Pipaslot.Mediator**, **Pipaslot.Mediator.Http**

- Handlers to be executed are available on MediatorContext 
- Returned back pipeline definition via `.AddPipeline(condition, p => { p.Use<...>().Use<...>(); })`. Pipelines replace default middleware if the pipeline condition is met.
- New V3 Serializer supporting interface serialization
- New methods `AddCredibleResultAssemblyOf<T>()` and `AddCredibleResultAssembly()` for registering types from the assembly as credible: `services.AddMediatorClient(o => o.AddCredibleResultAssemblyOf<MyCustomDTO>())`
- Added `IMediatorConfiguration.AddHandlers` `IMediatorConfiguration.AddActions` for direct action and handler registration by specifying exact types instead of scanning the whole assembly
- The object `MediatorContext` has a new `Status` property used for error detection during pipeline processing. Error messages are marked as obsolete and will be replaced by Notifications in the next major release.
- Error messages are not sent directly, but they are wrapped as Notification objects in the results collection with a specified Type (severity)


## Version 6.0.0
Nuget: **Pipaslot.Mediator**, **Pipaslot.Mediator.Http**

- Configuration for multi-handler execution in the pipeline was replaced by implementing `IConcurrentHandler` or `ISequenceHandler` interfaces on all handlers for the same action.
- Possibility to write custom HTTP response for case of file download from middlewares and handlers.
- Added `IMediatorContextAccessor` for accessing HTTP context from services out of middlewares and handlers.

### Breaking Changes
- Renamed `MediatorExceptionLoggingMiddleware` to `ExceptionLoggingMiddleware`
- Removed abstract class `ExecutionMiddleware`
- Middlewares `.UseSequenceMultiHandler()` (`MultiHandlerSequenceExecutionMiddleware`), `.UseConcurrentMultiHandler()` (`MultiHandlerConcurrentExecutionMiddleware`), `.UseSingleHandler()` (`SingleHandlerExecutionMiddleware`) were replaced by `HandlerExecutionMiddleware` supporting all handler type executions. Middleware `HandlerExecutionMiddlewar` is added automatically to the end of every pipeline if you configure in-process mediator usage.
- Renamed `PipelineConfigurator` to `MediatorConfigurator`
- Properties: `MediatorContext.ErrorMessages` and `MediatorContext.Results` were changed to read-only collections. Use methods `AddError()` or `AddResult()` for attaching own data
- Events `IMediator.ActionStarted` and `IMediator.ActionCompleted` were moved to `ActionEventsMiddleware`
- Removed `NotificationProviderMiddleware`. The functionality was integrated into the mediator.
- Pipeline registration via `.AddPipeline<>()` and `.AddDefaultPipeline()` was replaced by `.UseWhen(...)`. The default pipeline is implicitly involved in the mediator configurator.
- Interface `IConditionalPipelineConfigurator` was replaced by `IMiddlewareRegistrator`
- Removed Serializer from version 2


## Version 5.0.0
Nuget: **Pipaslot.Mediator**, **Pipaslot.Mediator.Http**

- Mediator server can be configured to provide different status codes (409, 500...) in case of error during processing
- Mediator server accepts HTTP GET requests. See `Pipaslot.Mediator.Http.IMediatorUrlFormatter`. It provides support for file download.
- New method `Mediator.ExecuteOrNew` returning data or a new instance depending on success status. It is an alternative to the method `Mediator.ExecuteOrDefault` returning null in case of failure.
- Added `IMediatorConfiguration.AddHandlers` `IMediatorConfiguration.AddActions` for direct action and handler registration by specifying exact types instead of scanning the whole assembly

### Fixed
- NotificationProviderMiddleware: Serving exception even if an exception was thrown during handler processing

### Breaking Changes
- API changed for HttpClientExecutionMiddleware
- API changed for IMediatorMiddleware: method `Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)` was replaced by `Invoke(MediatorContext context, MiddlewareDelegate next)`. Action and cancellation token are available as a context property.
- Interface `IPipelineConfigurator` was renamed to `IMediatorConfigurator`
- The class `MediatorContext` has a private constructor. If you need to create a new instance, you can clone the original context by the method `context.CopyEmpty()`

## Version 4.2.0
Nuget: **Pipaslot.Mediator**
 - Added interfaces for sending notifications in action results

## Version 4.1.0
Nuget: **Pipaslot.Mediator**
 - Custom pipeline condition via `.AddPipeline("Condition specified by class reflection", actionType => { ... })`

## Version 4.0.0
Nuget: **Pipaslot.Mediator**, **Pipaslot.Mediator.Http**
 - The client mediator supports its own pipeline, action, and handler registration, but by default, it sends actions to the server via the HTTP protocol
 - Serialization logic from the server and client was extracted into the services IActionSerializer and IResponseSerializer, which can be replaced via DI re-configuration
 - New data serializer was implemented supporting full JSON support
 - IMediatorResponse has a new property `Results` as an object array type providing messages from middlewares
 - All middlewares (except execution middlewares) can have configured ServiceLifetime (default is scoped)
 - ServiceLifetime for handlers can be changed (default is transient)
 - The mediator provides events ActionStarted and ActionCompleted notifying subscribers when a new action is started and completed. These events also provide a collection of all running actions.
 - New method Mediator.ExecuteOrDefault returning data or default object depending on success status
 - Added configurable middleware UseReduceDuplicateProcessing for reducing concurrent action calls to minimize server load 

### Breaking changes
 - Obsolete code from the previous version was removed
 - NuGet packages `Pipaslot.Mediator.Client` and `Pipaslot.Mediator.Server` were replaced by `Pipaslot.Mediator.Http`
 - Service collection method AddMediatorClient returns IPipelineConfigurator instead of IServiceColelction
 - ClientMediator removed was replaced by HttpClientExecutionMiddleware
 - Contracts `MediatorRequestSerializable`, `MediatorResponseDeserialized`, and `MediatorResponseSerializable` were moved from Pipaslot.Mediator.Contracts to `Pipaslot.Mediator.Http.Contracts`
 - Removed support for contract serialization implemented in Version 1
 - Property ServerMediatorOptions.KeepCompatibilityWithVersion1 was removed
 - Class RequestContractExecutor was removed, and the behavior was moved to MediatorMiddleware and IContractSerializer
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
