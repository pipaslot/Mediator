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

        [Obsolete("Will be deleted in next version")]
        public RuleSetDto[] RuleSets { get; set; } = Array.Empty<RuleSetDto>();

        [Obsolete("Will be deleted in next version")]
        public class RuleSetDto
        {
            public string Operator { get; set; } = string.Empty;
            public RuleSetDto[] SubSets { get; set; } = Array.Empty<RuleSetDto>();
            public RuleDto[] Rules { get; set; } = Array.Empty<RuleDto>();
        }

        [Obsolete("Will be deleted in next version")]
        public class RuleDto
        {
            public string Name { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public bool Granted { get; set; }
        }
    }
}
