# Mediator
Mediator pattern over HTTP for Blazor WASM.
Define action contract on a shared project, define action handler on server-side and fire the action from the client. The mediator will carry about all of the communication for you.

This package was designed for fast and flexible development of simultaneously developed client and server applications.

Documentation:
 - [Wiki](https://github.com/pipaslot/Mediator/wiki)
 - [Release notes and breaking changes](https://github.com/pipaslot/Mediator/wiki/Release-notes-and-breaking-changes)
 - [Basic sample for Blazor WASM](https://github.com/pipaslot/Mediator/wiki/1.-Basic-usage:-in-process)


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


#### Error Handling
Mediator catches all exception which occures during midleware or action handler execution. If you want to provide Logging via ILogger interface, you can create own logging middleware or register already prepared middleware in your pipelines by `.UseExceptionLogging()`
Keep in mind that mediator handles all unhandled exceptions internally. That means that your ASP.NET Core middleware handling exceptions for example from ASP.NET Core MVC wont receive these exceptions.
