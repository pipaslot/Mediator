namespace Pipaslot.Mediator.Middlewares
{
    /// <summary>
    /// Specify how the action is binded to handler
    /// </summary>
    public enum ActionToHandlerBindingType
    {
        /// <summary>
        /// By action class name inheriting IMediatorAction
        /// </summary>
        Class,
        /// <summary>
        /// By action interface inheriting IMediatorAction
        /// </summary>
        Interface
    }
}
