namespace Pipaslot.Mediator.Authorization
{
    public interface IHandlerAuthorization<TAction> : IHandlerAuthorizationMarker
    {
        public IPolicy Authorize(TAction action);
    }
}
