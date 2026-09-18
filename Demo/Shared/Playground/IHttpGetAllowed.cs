namespace Demo.Shared.Playground;

/// <summary>
/// Marker interface for GET requests preventing no other action can execute mediator via HTTP GET to prevent CSRF attacks
/// </summary>
public interface IHttpGetAllowed;