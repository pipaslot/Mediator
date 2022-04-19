using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pipaslot.Mediator.Http.Configuration
{
    public class ClientMediatorOptions
    {
        private List<Type> _credibleResultTypes = new List<Type>()
        {
            typeof(Notification)
        };
        private List<Assembly> _credibleResultAssemblies = new List<Assembly>();

        public string Endpoint { get; set; } = MediatorConstants.Endpoint;
        /// <summary>
        /// Protect deserialization process by check whether target type is credible. 
        /// Prevents agains exploiting this feature by attackers. Disabled by default.
        /// </summary>
        public bool DeserializeOnlyCredibleResultTypes { get; set; } = false;

        public SerializerType SerializerTyoe { get; set; } = SerializerType.V2;

        /// <summary>
        /// Define extra types returned by server as credible.
        /// For action result type refistrations use mediator methods <see cref="IMediatorConfigurator.AddActionsFromAssemblyOf"/> or <see cref="IMediatorConfigurator.AddActionsFromAssembly"/>
        /// </summary>
        //TODO Convert to IEnumerable
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
        /// Define extra types returned by server as credible.
        /// For action result type refistrations use mediator methods <see cref="IMediatorConfigurator.AddActionsFromAssemblyOf"/> or <see cref="IMediatorConfigurator.AddActionsFromAssembly"/>
        /// </summary>
        public IEnumerable<Assembly> CredibleResultAssemblies
        {
            get => _credibleResultAssemblies;
            set
            {
                _credibleResultAssemblies.Clear();
                _credibleResultAssemblies.AddRange(value);
            }
        }

        /// <summary>
        /// Define extra types as trusted for deserialization. Set <see cref="DeserializeOnlyCredibleResultTypes"/> to true
        /// </summary>
        /// <typeparam name="T">Seed type from target assembly</typeparam>
        public ClientMediatorOptions AddCredibleResultType<T>()
        {
            _credibleResultTypes.Add(typeof(T));
            DeserializeOnlyCredibleResultTypes = true;
            return this;
        }

        /// <summary>
        /// Define extra assembly as trusted for deserialization. Set <see cref="DeserializeOnlyCredibleResultTypes"/> to true
        /// </summary>
        /// <typeparam name="T">Seed type from target assembly</typeparam>
        /// <returns></returns>
        public ClientMediatorOptions AddCredibleResultAssemblyOf<T>()
        {
            _credibleResultAssemblies.Add(typeof(T).Assembly);
            DeserializeOnlyCredibleResultTypes = true;
            return this;
        }

        /// <summary>
        /// Define extra assembly as trusted for deserialization. Set <see cref="DeserializeOnlyCredibleResultTypes"/> to true
        /// </summary>
        public ClientMediatorOptions AddCredibleResultAssembly(params Assembly[] assemblies)
        {
            _credibleResultAssemblies.AddRange(assemblies);
            DeserializeOnlyCredibleResultTypes = true;
            return this;
        }
    }
}
