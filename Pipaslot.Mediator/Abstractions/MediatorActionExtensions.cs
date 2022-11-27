namespace Pipaslot.Mediator.Abstractions
{
    internal static class MediatorActionExtensions
    {
        public static string GetActionName(this IMediatorAction action)
        {
            return action.GetType().ToString();
        }
    }
}
