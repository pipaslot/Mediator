namespace Pipaslot.Mediator.Services
{
    public interface IHandlerExistenceChecker
    {
        void Verify(bool checkMatchingHandlers = false, bool checkExistingPolicies = false);
    }
}