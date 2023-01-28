using System;
#if !NETSTANDARD2_0
using System.Text.Json.Serialization;
#endif


namespace Pipaslot.Mediator.Authorization
{
    public class AuthorizeRequestResponse
    {
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

#if !NETSTANDARD2_0
        /// <summary>
        /// Returns true when is Allowed or Deny
        /// </summary>
        [JsonIgnore]
        public bool IsAvailable => Access == AccessType.Allow || Access == AccessType.Deny;

        [JsonIgnore]
        public bool IsAllowed => Access == AccessType.Allow;

        /// <summary>
        /// The Access is Unavailable or Deny
        /// </summary>
        [JsonIgnore]
        public bool IsDisabled => Access == AccessType.Unavailable || Access == AccessType.Deny;
#endif
    }
}
