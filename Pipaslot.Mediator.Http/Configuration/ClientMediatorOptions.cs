using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Http.Configuration
{
    public class ClientMediatorOptions
    {
        private List<Type> _credibleResultTypes = new List<Type>()
        {
            typeof(Notification)
        };

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
        public Type[] CredibleResultTypes
        {
            get => _credibleResultTypes.ToArray();
            set
            {
                _credibleResultTypes.Clear();
                _credibleResultTypes.AddRange(value);
            }
        }

        public ClientMediatorOptions AddCredibleResultType<T>()
        {
            _credibleResultTypes.Add(typeof(T));
            return this;
        }
    }
}
