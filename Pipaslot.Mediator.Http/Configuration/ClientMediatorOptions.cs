using Pipaslot.Mediator.Configuration;
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

        public SerializerType SerializerTyoe { get; set; } = SerializerType.V2;

        /// <summary>
        /// Define extra types returned by server as credible. Setter does not replace existing values but append another ones.
        /// For action result type refistrations use mediator methods <see cref="IMediatorConfigurator.AddActionsFromAssemblyOf"/> or <see cref="IMediatorConfigurator.AddActionsFromAssembly"/>
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

        /// <summary>
        /// Define extra types returned by server as credible and set <see cref="DeserializeOnlyCredibleResultTypes"/> to true
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ClientMediatorOptions AddCredibleResultType<T>()
        {
            _credibleResultTypes.Add(typeof(T));
            DeserializeOnlyCredibleResultTypes = true;
            return this;
        }
    }
}
