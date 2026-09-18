namespace Demo.Shared.Playground;

/// <summary>
/// Marker interface for actions that are safe to invoke over HTTP GET (e.g. file downloads). Registered with
/// <c>ServerMediatorOptions.AllowHttpGetWhen(action => action is IHttpGetAllowed)</c> in Program.cs so that only
/// actions implementing this interface can be triggered via GET, protecting the rest from CSRF.
/// </summary>
public interface IHttpGetAllowed;
