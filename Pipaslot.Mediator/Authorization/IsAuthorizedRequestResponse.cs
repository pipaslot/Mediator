using System;

namespace Pipaslot.Mediator.Authorization
{
    public class IsAuthorizedRequestResponse
    {
        public AccessType Access { get; set; }
        public bool IsAuthorized { get; set; }

        /// <summary>
        /// User friendly explanation of the result
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// If TRUE, then the authorization result is supposed to be change when user login/logout or when his app-specific permission will change.
        /// This value is calculated from RuleSets
        /// </summary>
        public bool IsIdentityStatic { get; set; }
    }
}
