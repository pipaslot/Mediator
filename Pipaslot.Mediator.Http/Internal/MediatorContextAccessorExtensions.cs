namespace Pipaslot.Mediator.Http.Internal
{
    internal static class MediatorContextAccessorExtensions
    {
        internal static bool IsFirstAction(this IMediatorContextAccessor accessor)
        {
            // We test for count to be 1 because the actual action is already counted
            return accessor.ContextStack.Count == 1;
        }
    }
}
