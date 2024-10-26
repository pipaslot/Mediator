using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Abstractions;

/// <summary>
/// Top level action handler marker not returning any data. 
/// </summary>
/// <typeparam name="TAction">Action type to be processed</typeparam>
public interface IMediatorHandler<in TAction> where TAction : IMediatorAction
{
    /// <summary>Handles an message</summary>
    /// <param name="action">The action to be processed containing input parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task Handle(TAction action, CancellationToken cancellationToken);
}

/// <summary>
/// Top level action handler marker returning data in case of successfull execution. 
/// </summary>
/// <typeparam name="TAction">Action type to be processed</typeparam>
/// <typeparam name="TResult">Result type returned by the handler to be provided by mediator</typeparam>
public interface IMediatorHandler<in TAction, TResult> where TAction : IMediatorAction<TResult>
{
    /// <summary>Handles an action and return result</summary>
    /// <param name="action">The action to be processed containing input parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response from the request</returns>
    Task<TResult> Handle(TAction action, CancellationToken cancellationToken);
}