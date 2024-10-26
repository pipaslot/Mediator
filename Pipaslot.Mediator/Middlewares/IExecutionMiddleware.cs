namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Default interface marking middleware as last middleware in pipeline executing handlers or other operations with actions.
/// As service registered in service collection represents default execution middleware used if no pipeline is defined or the pipeline does not specify execution middleware.
/// </summary>
public interface IExecutionMiddleware : IMediatorMiddleware
{
}