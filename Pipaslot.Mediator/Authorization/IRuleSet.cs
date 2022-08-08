namespace Pipaslot.Mediator.Authorization
{
    public interface IRuleSet
    {
        bool Granted { get; }
        string StringifyNotGranted();
    }
}
