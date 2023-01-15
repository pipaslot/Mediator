namespace Pipaslot.Mediator.Authorization
{
    public class AuthorizeRequestResponse
    {
        public AccessType Access { get; set; }
        /// <summary>
        /// Access type converted to bool
        /// </summary>
        public bool IsAuthorized => Access == AccessType.Allow;

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
