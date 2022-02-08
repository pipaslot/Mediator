using Pipaslot.Mediator.Notifications;
using System;

namespace Pipaslot.Mediator.Http.Configuration
{
    public class ClientMediatorOptions
    {
        public string Endpoint { get; set; } = MediatorConstants.Endpoint;
        /// <summary>
        /// Protect deserialization process by check whether target type is credible. 
        /// Prevents agains exploiting this feature by attackers. Disabled by default.
        /// </summary>
        public bool DeserializeOnlyCredibleResultTypes { get; set; } = false;

        /// <summary>
        /// Define extra types returned by server as credible. 
        /// For action result type refistrations use mediator methods AddActionsFromAssemblyOf or AddActionsFromAssembly
        /// </summary>
        public Type[] CredibleResultTypes { get; set; } = new Type[] { typeof(Notification) };
    }
}
