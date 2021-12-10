# Mediator
Mediator pattern over HTTP for Blazor WASM.
Define action contract on a shared project, define action handler on server-side and fire the action from the client. The mediator will carry about all of the communication for you.

This package was designed for fast and flexible development of simultaneously developed client and server applications.

Documentation:
 - [Wiki](https://github.com/pipaslot/Mediator/wiki)
 - [Release notes and breaking changes](https://github.com/pipaslot/Mediator/wiki/Release-notes-and-breaking-changes)
 - [Basic sample for Blazor WASM](https://github.com/pipaslot/Mediator/wiki/1.-Basic-usage:-in-process)


## Pipelines
Pipelines are optional. The purpose is to provide different processing for different action types. For example you want to apply caching for Request reponses which should not affect messages. 
From oposite site you want to audit all messages sent throug mediator but do not want to audit Requests. To gain the expected result we will define two pipelines.
``` .AddMediatorServer()
    ... //Action and handler registrations
    .AddPipeline<IMessage>()
        .Use<AuditMessageMediatorMiddleware()
        .Use<AnotherMessageSpecificMediatorMiddleware>()
    .AddPipeline<IRequest>()
        .Use<CacheRequestResponseMediatorMiddleware()
```
For more complex sample we may decide to audit only some specific Messages which has interface `IAuditableMessage` inheriting from`IMessage`. In this case we would update the mediator configuration to:
``` .AddMediatorServer()
    ... //Action and handler registrations
    .AddPipeline<IAuditableMessage>()
        .Use<AuditMessageMediatorMiddleware()
        .Use<AnotherMessageSpecificMediatorMiddleware>()
    .AddPipeline<IMessage>()
        .Use<AnotherMessageSpecificMediatorMiddleware>()
    .AddPipeline<IRequest>()
        .Use<CacheRequestResponseMediatorMiddleware()
```
Pay attention on the ordering in pipeline definitions! `IAuditableMessage` is more specific type. If we would register `AddPipeline<IMessage>()` before `AddPipeline<IAuditableMessage>()`, pipeline for `IAuditableMessage` would never been used!

### Multiple handlers per action contract
Sometimes multiple actions are expected to be executed. For example, you would like to forward the message to another audit server meanwhile the message is processed on your server. 
Or you would like to chain another action-handler once the first was finished.

For this purpose you can register middlewarer in mediator with `UseConcurrentMultiHandler()` and `UseSequenceMultiHandler()`
``` .AddMediatorServer()
    ... //Action and handler registrations
    .AddPipeline<...>()
        .UseConcurrentMultiHandler()
    .AddPipeline<...>() 
        .UseSequenceMultiHandler()
```

If you need to define the order of how the sequence handlers will be executed, then implement interface `ISequenceHandler` in your handlers and define ordering.

### Single handler for multiple actions (interface handler)
TODO

### Create custom action types
Don't you like naming as Request and Message? Or do you want to provide more action types to cover your specific pipeline behaviour? Just defino own Action and message types.
Here is example of naming for CQRS (Command and Query Responsibility Segregation)

ICommand.cs
```
using Pipaslot.Mediator;

namespace CustomMediator.Cqrs
{
    /// <summary>
    /// Marker type for actions and pipelines
    /// </summary>
    public interface ICommand :  : IMediatorAction
    {
    }
}
```

ICommandHandler.cs
```
using Pipaslot.Mediator.Abstraction;

namespace CustomMediator.Cqrs
{
    public interface ICommandHandler<TCommand> : IMediatorHandler<TCommand> where TCommand : ICommand
    {

    }
}
```

IQuery.cs
```
using Pipaslot.Mediator;

namespace CustomMediator.Cqrs
{
    /// <summary>
    /// Marker type for actions
    /// </summary>
    public interface IQuery<TResult> : IQuery, IMediatorAction<TResult>
    {
    }
    /// <summary>
    /// Marker type for pipelines
    /// </summary>
    public interface IQuery : IMediatorAction
    {
    }
}
```

IQueryHandler.cs
```
using Pipaslot.Mediator.Abstraction;

namespace CustomMediator.Cqrs
{
    public interface IQueryHandler<TQuery, TResult> : IMediatorHandler<TQuery, TResult> where TQuery: IQuery<TResult>
    {

    }
}

```

And that is it. Easy!

### Middlewares
Middlewares provides you a possibility to intercept action exectuon. With middleares you can achieve  for exampe:caching, auditing, logging, pre-processing, post-processing, transactions ...

Simplest middleware lookis like this:

```
public class DummyMiddleware : IMediatorMiddleware
{
    public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
    {
        // Here place code executed before handler call
        await next(context);
        // Here place code executed after handler call
    }
}
```

Do not forget to ensure that you execute `await next(context);`, otherwise neither next middleware nor handler would be executed.
Handler executing is also realised by build-in middleware .UseSingleHandler() used as default. For MediatorClient it is .UseHttpClient() by default


#### Error Handling
Mediator catches all exception which occures during midleware or action handler execution. If you want to provide Logging via ILogger interface, you can create own logging middleware or register already prepared middleware in your pipelines by `.UseExceptionLogging()`
Keep in mind that mediator handles all unhandled exceptions internally. That means that your ASP.NET Core middleware handling exceptions for example from ASP.NET Core MVC wont receive these exceptions.
