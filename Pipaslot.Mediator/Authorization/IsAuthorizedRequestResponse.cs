using System;

namespace Pipaslot.Mediator.Authorization
{
    public class IsAuthorizedRequestResponse
    {
        public bool IsAuthorized { get; set; }

        /// <summary>
        /// If TRUE, then the authorization result is supposed to be change when user login/logout or when his app-specific permission will change.
        /// This value is calculated from RuleSets
        /// </summary>
        public bool IsIdentityStatic { get; set; }

        public RuleSetDto[] RuleSets { get; set; } = Array.Empty<RuleSetDto>();

        public class RuleSetDto
        {
            public string Operator { get; set; } = string.Empty;
            public RuleSetDto[] SubSets { get; set; } = Array.Empty<RuleSetDto>();
            public RuleDto[] Rules { get; set; } = Array.Empty<RuleDto>();
        }

        public class RuleDto
        {
            public string Name { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public bool Granted { get; set; }
        }
    }
}
