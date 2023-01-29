using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Services
{
    public class ExistenceCheckerSetting
    {
        public bool CheckMatchingHandlers { get; set; }
        public bool CheckExistingPolicies { get; set; }
        public HashSet<Type> IgnoredPolicyChecks { get; set; } = new();
    }
}