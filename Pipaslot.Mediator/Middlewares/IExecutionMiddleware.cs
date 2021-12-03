namespace Pipaslot.Mediator.Middlewares
{
    /// <summary>
    /// Default interface marking middleware as last middleware in pipeline executing handlers or other operations with actions
    /// </summary>
    public interface IExecutionMiddleware : IMediatorMiddleware
    {
    }
}
