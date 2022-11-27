namespace Pipaslot.Mediator.Authorization
{
    public interface IActionAuthorization
    {
        public IPolicy Authorize();
    }
}
