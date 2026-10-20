using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// Result of <see cref="AuthorizeRequest"/>, returned by <see cref="MediatorExtensions.Authorize"/>.
/// </summary>
public class AuthorizeRequestResponse
{
    /// <summary>
    /// <see cref="AccessType.Unavailable"/> until evaluated; <see cref="MediatorExtensions.Authorize"/> also returns it when
    /// the request itself fails.
    /// </summary>
    public AccessType Access { get; set; } = AccessType.Unavailable;

    /// <summary>
    /// User friendly explanation of the result
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// If TRUE, then the authorization result is supposed to be change when user login/logout or when his app-specific permission will change.
    /// This value is calculated from RuleSets
    /// </summary>
    public bool IsIdentityStatic { get; set; }

    /// <summary>
    /// Returns true when is Allowed or Deny
    /// </summary>
    [JsonIgnore]
    public bool IsAvailable => Access is AccessType.Allow or AccessType.Deny;

    /// <summary>
    /// True only for <see cref="AccessType.Allow"/> - the same answer as <see cref="MediatorExtensions.IsAuthorized"/>.
    /// </summary>
    [JsonIgnore]
    public bool IsAllowed => Access == AccessType.Allow;

    /// <summary>
    /// The Access is Unavailable or Deny
    /// </summary>
    [JsonIgnore]
    public bool IsDisabled => Access is AccessType.Unavailable or AccessType.Deny;
}