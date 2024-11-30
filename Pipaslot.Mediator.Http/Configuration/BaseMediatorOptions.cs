using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pipaslot.Mediator.Http.Configuration;

public class BaseMediatorOptions<TBuilder> : IMediatorOptions where TBuilder : BaseMediatorOptions<TBuilder>
{
    private string _endpoint = MediatorConstants.Endpoint;

    public string Endpoint
    {
        get => _endpoint;
        set
        {
            var notNulValue = (value ?? "").Trim();
            _endpoint = notNulValue.StartsWith("/") ? notNulValue : $"/{notNulValue}";
        }
    }

    /// <inheritdoc/>
    public bool IgnoreReadOnlyProperties { get; set; }

    /// <summary>
    /// Protect deserialization process by check whether target type is credible. 
    /// Prevents agains exploiting this feature by attackers. Disabled by default.
    /// </summary>
    public bool DeserializeOnlyCredibleResultTypes { get; set; }

    public SerializerType SerializerType { get; set; } = SerializerType.V3;

    #region Credible types

    private readonly List<Type> _credibleResultTypes = [typeof(Notification)];
    private readonly List<Assembly> _credibleResultAssemblies = [];

    /// <summary>
    /// Define extra types returned by server as credible.
    /// For action result type refistrations use mediator methods <see cref="IMediatorConfigurator.AddActionsFromAssemblyOf"/> or <see cref="IMediatorConfigurator.AddActionsFromAssembly"/>
    /// </summary>
    public IEnumerable<Type> CredibleResultTypes
    {
        get => _credibleResultTypes;
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
    public TBuilder AddCredibleResultType<T>()
    {
        _credibleResultTypes.Add(typeof(T));
        DeserializeOnlyCredibleResultTypes = true;
        return (TBuilder)this;
    }

    /// <summary>
    /// Define extra assembly as trusted for deserialization. Set <see cref="DeserializeOnlyCredibleResultTypes"/> to true
    /// </summary>
    /// <typeparam name="T">Seed type from target assembly</typeparam>
    /// <returns></returns>
    public TBuilder AddCredibleResultAssemblyOf<T>()
    {
        _credibleResultAssemblies.Add(typeof(T).Assembly);
        DeserializeOnlyCredibleResultTypes = true;
        return (TBuilder)this;
    }

    /// <summary>
    /// Define extra assembly as trusted for deserialization. Set <see cref="DeserializeOnlyCredibleResultTypes"/> to true
    /// </summary>
    public TBuilder AddCredibleResultAssembly(params Assembly[] assemblies)
    {
        _credibleResultAssemblies.AddRange(assemblies);
        DeserializeOnlyCredibleResultTypes = true;
        return (TBuilder)this;
    }

    #endregion
}