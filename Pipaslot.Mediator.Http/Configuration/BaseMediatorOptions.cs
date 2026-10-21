using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pipaslot.Mediator.Http.Configuration;

/// <summary>
/// Settings shared by the client (<see cref="ClientMediatorOptions"/>) and the server (<see cref="ServerMediatorOptions"/>)
/// side of the HTTP transport.
/// </summary>
/// <remarks>
/// Client and server must agree on <see cref="Endpoint"/>. The credible-type members configure the deserialization
/// allowlist (<see cref="ICredibleProvider"/>) that protects against a payload naming an arbitrary type to instantiate;
/// that decision is independent of the JSON shape produced by the serializer.
/// </remarks>
/// <typeparam name="TBuilder">The concrete options type, returned by the fluent <c>AddCredible*</c> methods.</typeparam>
public abstract class BaseMediatorOptions<TBuilder> : IMediatorOptions where TBuilder : BaseMediatorOptions<TBuilder>
{
    /// <summary>
    /// URL path of the single endpoint through which all actions travel. The client posts to it and the server
    /// middleware answers on it, so both sides need the same value. A missing leading slash is added.
    /// Defaults to <see cref="MediatorConstants.Endpoint"/>.
    /// </summary>
    public string Endpoint
    {
        get;
        set
        {
            var notNulValue = (value ?? "").Trim();
            field = notNulValue.StartsWith("/") ? notNulValue : $"/{notNulValue}";
        }
    } = MediatorConstants.Endpoint;

    /// <inheritdoc/>
    public bool IgnoreReadOnlyProperties { get; set; }

    /// <summary>
    /// Protect deserialization process by check whether the target type is credible.
    /// Prevents against exploiting this feature by attackers. Disabled by default.
    /// </summary>
    /// <remarks>
    /// Turns on the <see cref="ICredibleProvider"/> allowlist for result types, on top of the one always applied to
    /// incoming actions. Result types of registered actions pass automatically; anything else has to be added through
    /// <see cref="AddCredibleResultType{T}"/>/<see cref="AddCredibleResultAssemblyOf{T}"/>, each of which turns this flag
    /// on as a side effect. Leaving it disabled means the client will deserialize whatever type name the response names.
    /// </remarks>
    public bool DeserializeOnlyCredibleResultTypes { get; set; }
    
    /// <summary>
    /// Register <see cref="IMediatorContextAccessor"/> and <see cref="INotificationProvider"/> needed for context accessing out of the mediator middlewares.
    /// If performance matter, and you do not need to: <br />
    /// - access the <see cref="MediatorContext"/> <br />
    /// - Use service <see cref="MediatorFacade"/> <br />
    /// - track nested calls <br />
    /// - send notification from nested calls <br />
    /// - access the context out of mediator handler<br />
    /// Then you can set this parameter to False and you will save CPU time 70ns and avoid allocation of 380bytes per mediator call.
    /// </summary>
    public virtual bool AddContextAccessor { get; set; } = true;

    #region Credible types

    private readonly List<Type> _credibleResultTypes = [typeof(Notification)];
    private readonly List<Assembly> _credibleResultAssemblies = [];

    /// <summary>
    /// Define extra types returned by server as credible.
    /// For action result type refistrations use mediator methods <see cref="IMediatorConfigurator.AddActionsFromAssemblyOf"/> or <see cref="IMediatorConfigurator.AddActionsFromAssembly"/>
    /// </summary>
    /// <remarks>
    /// Setting the property replaces the whole list, including the built-in <see cref="Notification"/> entry; prefer
    /// <see cref="AddCredibleResultType{T}"/>, which appends. Adding types here has an effect only while the matching
    /// <c>DeserializeOnlyCredible*</c> flag is enabled.
    /// </remarks>
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
    /// <remarks>
    /// Trusts every type of the listed assemblies, so list only assemblies that contain nothing but contracts your
    /// application intends to receive; prefer <see cref="CredibleResultTypes"/> for individual types. Setting the
    /// property replaces the whole list; <see cref="AddCredibleResultAssemblyOf{T}"/> appends instead.
    /// </remarks>
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