What to change in your code when upgrading `Pipaslot.Mediator`/`Pipaslot.Mediator.Http` across a major version. Upgrading over several majors at once, apply the sections from your current version upwards. Minor and patch releases are non-breaking; the full list of changes of every version is in [GitHub Releases](https://github.com/pipaslot/Mediator/releases), the changelog up to 9.1.0 in the [archived release notes](https://github.com/pipaslot/Mediator/blob/main/docs/archive/Release-notes-and-breaking-changes.md).

## Upgrading to 9.0
* `Dispatch`/`Execute` no longer copy a caught exception's `Message` into the response; an exception without a registered handler produces a generic message and an `Error`-level log entry — see [Safe by default](6.2.-Exception-handling.md#safe-by-default). To restore the old behavior, register the opt-in `LegacyExceptionMessageHandler` — see [Restore the pre-safe-by-default message behavior](6.2.-Exception-handling.md#restore-the-pre-safe-by-default-message-behavior-temporarily).
* A middleware catching exceptions to convert them into error messages should be replaced by typed exception handlers — see [Migrating from a catch-all ErrorHandlingMiddleware](6.2.-Exception-handling.md#migrating-from-a-catch-all-errorhandlingmiddleware).
* Adding an error-typed `Notification` via `MediatorContext.AddResult`/`INotificationProvider.Add` (and the `AddError`/`AddWarning`/etc. helpers built on it) no longer sets `MediatorContext.Status` to `Failed` by itself — only `MediatorContext.AddError()`/`AddErrors()` do that now. See [Notifications in server response](9.4.-Cookbook-and-integrations.md#notifications-in-server-response).
* `AddMediatorClient` no longer registers `IMediatorContextAccessor`/`INotificationProvider`/`IMediatorFacade` by default; opt back in with `services.AddMediatorClient(o => o.AddContextAccessor = true)` — see [Client specific](8.-HTTP-transport-and-configuration-for-Client-Server-usage.md#client-specific).
* `AddHandlers`/`AddHandlersFromAssemblyOf<T>` now take `ServiceLifetime? serviceLifetime = null` instead of defaulting to `ServiceLifetime.Transient`; an explicit `ServiceLifetime.Transient` for a handler implementing `ISingleton`/`IScoped` now throws `MediatorException` instead of being silently overridden — see [Handler ServiceLifetime](6.-Pipelines-and-Middlewares.md#handler-servicelifetime).
* `Pipaslot.Mediator.Http.Configuration.BaseMediatorOptions<TBuilder>` is now `abstract` and can no longer be instantiated directly.
* `HandlerExistenceChecker`'s constructor no longer takes a `MediatorConfigurator` parameter; resolve it from DI rather than constructing it directly.
* `MediatorConfigurator` no longer implements `IActionTypeProvider`; the related methods are obsolete.
* `MediatorExecutionException.CreateForUnhandledError` (both overloads) was removed; use `MediatorUnhandledErrorException.Create` instead.
* `ReduceDuplicateProcessingMiddleware` and `MiddlewareRegistratorExtensions.UseReduceDuplicateProcessing` were removed; implement your own middleware with an explicit dedup key instead.
* `ServiceProviderExtensions` is now `internal`, and its `GetActionHandlers` was removed.
* `Pipaslot.Mediator.Abstractions.RequestGenericHelpers` was removed.

## Upgrading to 8.0
* Support for .NET Standard 2.0 was dropped.
* `IRule`, `IRuleFormatter`, `EvaluatedRule` and `IEvaluatedRule` were removed. Rules and their combinations in `RuleSet`s are now converted to nodes with an evaluated `RuleOutcome`, pruned, and then formatted.
* `DefaultRuleFormatter` was replaced by `DefaultNodeFormatter`; use it as the template for a custom formatter — see [Authorization](7.-Authorization.md).
* Serializer V2 was removed, including the serializer type selection.
* The default HTTP status code of an error response is 500 (previously 200) — see [Server: choosing the HTTP status code](8.-HTTP-transport-and-configuration-for-Client-Server-usage.md#server-choosing-the-http-status-code).

## Upgrading to 7.0
* `RuleSetReproducibility` was replaced by `RuleScope`.
* Authorization: the grant calculation was replaced by `RuleOutcome` and `AccessType` (the `RuleSet` API changed).
* The action and handler `IsAuthorizedRequest` was renamed to `AuthorizeRequest`.
* The `IHandlerExistenceChecker.Verify` signature changed.
* `NotificationType.ActionError` was removed; an exception caught during handler execution is no longer sent as a notification.
* The obsolete properties `IMediatorResponse.ErrorMessage` and `IMediatorResponse.ErrorMessages` were removed.
* Serializer V3 is the default.

## Upgrading to 6.0
* `MediatorExceptionLoggingMiddleware` was renamed to `ExceptionLoggingMiddleware`.
* The abstract class `ExecutionMiddleware` was removed.
* The middlewares `.UseSequenceMultiHandler()` (`MultiHandlerSequenceExecutionMiddleware`), `.UseConcurrentMultiHandler()` (`MultiHandlerConcurrentExecutionMiddleware`) and `.UseSingleHandler()` (`SingleHandlerExecutionMiddleware`) were replaced by `HandlerExecutionMiddleware`, added automatically to the end of every in-process pipeline. Multi-handler execution is configured by implementing `IConcurrentHandler` or `ISequenceHandler` on all handlers of the action — see [Multi-handler execution](9.2.-Multi-handler-execution.md).
* `PipelineConfigurator` was renamed to `MediatorConfigurator`.
* `MediatorContext.ErrorMessages` and `MediatorContext.Results` are read-only collections; use `AddError()` or `AddResult()` to attach data.
* The events `IMediator.ActionStarted` and `IMediator.ActionCompleted` were moved to `ActionEventsMiddleware`.
* `NotificationProviderMiddleware` was removed; its functionality is built into the mediator.
* Pipeline registration via `.AddPipeline<>()` and `.AddDefaultPipeline()` was replaced by `.UseWhen(...)`; the default pipeline is implicit.
* `IConditionalPipelineConfigurator` was replaced by `IMiddlewareRegistrator`.
* The serializer from version 2 was removed.

## Upgrading to 5.0
* The API of `HttpClientExecutionMiddleware` changed.
* `IMediatorMiddleware.Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)` was replaced by `Invoke(MediatorContext context, MiddlewareDelegate next)`; the action and the cancellation token are properties of the context — see [Pipelines and Middlewares](6.-Pipelines-and-Middlewares.md).
* `IPipelineConfigurator` was renamed to `IMediatorConfigurator`.
* `MediatorContext` has a private constructor; to create a new instance, clone an existing context with `context.CopyEmpty()`.

## Upgrading to 4.0
* The NuGet packages `Pipaslot.Mediator.Client` and `Pipaslot.Mediator.Server` were replaced by `Pipaslot.Mediator.Http`.
* Obsolete code from the previous version was removed, including the contract serialization of version 1 and `ServerMediatorOptions.KeepCompatibilityWithVersion1`.
* `AddMediatorClient` returns `IPipelineConfigurator` instead of `IServiceCollection`.
* `ClientMediator` was replaced by `HttpClientExecutionMiddleware`.
* The contracts `MediatorRequestSerializable`, `MediatorResponseDeserialized` and `MediatorResponseSerializable` were moved from `Pipaslot.Mediator.Contracts` to `Pipaslot.Mediator.Http.Contracts`.
* `RequestContractExecutor` was removed; its behavior moved to `MediatorMiddleware` and `IContractSerializer`.
* `Pipaslot.Mediator.Server.MediatorExceptionLoggingMiddleware` was replaced by `Pipaslot.Mediator.Http.Middlewares.ExceptionLoggingMiddleware`.
* `Pipaslot.Mediator.Server.MediatorServerException` was replaced by `Pipaslot.Mediator.Http.MediatorHttpException`.
* The pipeline configuration classes were moved to `Pipaslot.Mediator.Configuration`.

## Upgrading to 3.0
* `Pipaslot.Mediator.Client` 2.0.0 is not fully compatible with `Pipaslot.Mediator.Server` 3.0.0; upgrade both together.
* `IMessage`, `IRequest<T>`, `IMessageHandler` and `IRequestHandler<T>` became a naming layer over `IMediatorAction`, `IMediatorAction<T>`, `IMediatorHandler` and `IMediatorHandler<T>`; existing code using them keeps working.
