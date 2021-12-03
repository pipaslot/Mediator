using System;

namespace Pipaslot.Mediator.Middlewares
{
    /// <summary>
    /// Execute single handler with action defined as interface derived from IMediatorAction
    /// </summary>
    public class SingleInterfaceHandlerExecutionMiddleware : SingleHandlerExecutionMiddleware
    {
        public override ActionToHandlerBindingType BindingType { get; } = ActionToHandlerBindingType.Interface;
        public SingleInterfaceHandlerExecutionMiddleware(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
