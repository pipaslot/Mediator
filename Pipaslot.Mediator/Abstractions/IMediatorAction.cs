namespace Pipaslot.Mediator.Abstractions;

/// <summary>
/// Top level action marker for action without returning data. Connects actions returning result with those not not returning data to be processed by Mediator.
/// </summary>
public interface IMediatorAction
{
}

/// <summary>
/// Top level action marker for action which returns data. All derived types can have own specific pipelines and handlers.
/// </summary>
/// <typeparam name="TResult">Result data returned from handler execution</typeparam>
public interface IMediatorAction<out TResult> : IMediatorAction, IMediatorActionProvidingData
{
}

/// <summary>
/// FOR INTERNAL PURPOSE ONLY
/// </summary>
public interface IMediatorActionProvidingData
{
}