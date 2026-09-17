using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pipaslot.Mediator.Http.Configuration;

public class ServerMediatorOptions : BaseMediatorOptions<ServerMediatorOptions>
{
    /// <summary>
    /// Protect deserialization process by check whether target type is credible.
    /// Prevents agains exploiting this feature by attackers. Enabled by default.
    /// </summary>
    public bool DeserializeOnlyCredibleActionTypes { get; set; } = true;

    /// <summary>
    /// HTTP Status code returned in server response when mediator returns any error
    /// </summary>
    public int ErrorHttpStatusCode { get; set; } = MediatorConstants.ErrorHttpStatusCode;

    #region HTTP GET allowlist

    private readonly List<Type> _allowedHttpGetActionTypes = [];
    private readonly List<Assembly> _allowedHttpGetActionAssemblies = [];

    /// <summary>
    /// Restrict which action types <see cref="MediatorMiddleware"/> accepts over HTTP GET to the ones registered via
    /// <see cref="AddAllowedHttpGetActionType{T}"/>/<see cref="AddAllowedHttpGetActionAssemblyOf{T}"/>. Disabled by default.
    /// </summary>
    /// <remarks>
    /// A GET request can be triggered cross-site without a preflight check and without the caller's consent - an
    /// <c>&lt;img&gt;</c> or <c>&lt;link&gt;</c> tag pointing at the mediator endpoint is enough to make the victim's
    /// browser send it, cookies/Windows authentication included. This is a CSRF risk for any action that changes state.
    /// GET support exists for file downloads and similar read-only scenarios (see
    /// docs/wiki/9.3.-Custom-HTTP-responses-and-file-download.md) where the URL needs to be embeddable in an
    /// <c>&lt;a&gt;</c>/<c>&lt;img&gt;</c> tag; state-changing actions should not opt in.
    /// <para>
    /// Left disabled, every action type registered with the mediator can still be invoked over GET, unchanged from
    /// previous versions - this default keeps the fix backward compatible. It is planned to become enabled by default in
    /// the next major version; until then, enable it explicitly and register the action types (typically none, or only
    /// download-style queries) that are safe to trigger from a plain hyperlink or embedded resource.
    /// </para>
    /// </remarks>
    public bool RestrictHttpGetToAllowedActionTypes { get; set; }

    /// <summary>
    /// Action types allowed to be invoked over HTTP GET when <see cref="RestrictHttpGetToAllowedActionTypes"/> is enabled.
    /// </summary>
    public IEnumerable<Type> AllowedHttpGetActionTypes
    {
        get => _allowedHttpGetActionTypes;
        set
        {
            _allowedHttpGetActionTypes.Clear();
            _allowedHttpGetActionTypes.AddRange(value);
        }
    }

    /// <summary>
    /// Assemblies whose action types are allowed to be invoked over HTTP GET when
    /// <see cref="RestrictHttpGetToAllowedActionTypes"/> is enabled.
    /// </summary>
    public IEnumerable<Assembly> AllowedHttpGetActionAssemblies
    {
        get => _allowedHttpGetActionAssemblies;
        set
        {
            _allowedHttpGetActionAssemblies.Clear();
            _allowedHttpGetActionAssemblies.AddRange(value);
        }
    }

    /// <summary>
    /// Allow <typeparamref name="T"/> to be invoked over HTTP GET. Sets <see cref="RestrictHttpGetToAllowedActionTypes"/> to true.
    /// </summary>
    /// <typeparam name="T">Action type safe to trigger from a plain hyperlink or embedded resource (e.g. a file download query).</typeparam>
    public ServerMediatorOptions AddAllowedHttpGetActionType<T>() where T : IMediatorAction
    {
        _allowedHttpGetActionTypes.Add(typeof(T));
        RestrictHttpGetToAllowedActionTypes = true;
        return this;
    }

    /// <summary>
    /// Allow every action type declared in the assembly of <typeparamref name="T"/> to be invoked over HTTP GET.
    /// Sets <see cref="RestrictHttpGetToAllowedActionTypes"/> to true.
    /// </summary>
    /// <typeparam name="T">Seed type from the target assembly</typeparam>
    public ServerMediatorOptions AddAllowedHttpGetActionAssemblyOf<T>()
    {
        _allowedHttpGetActionAssemblies.Add(typeof(T).Assembly);
        RestrictHttpGetToAllowedActionTypes = true;
        return this;
    }

    /// <summary>
    /// Allow every action type declared in <paramref name="assemblies"/> to be invoked over HTTP GET.
    /// Sets <see cref="RestrictHttpGetToAllowedActionTypes"/> to true.
    /// </summary>
    public ServerMediatorOptions AddAllowedHttpGetActionAssembly(params Assembly[] assemblies)
    {
        _allowedHttpGetActionAssemblies.AddRange(assemblies);
        RestrictHttpGetToAllowedActionTypes = true;
        return this;
    }

    #endregion
}
