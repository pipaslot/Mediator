using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Authorization
{
    public class AuthorizationException : Exception
    {
        internal const int NoAuthorizationCode = 1;
        internal const int UnauthorizedHandlerCode = 2;
        internal const int RuleNotMetCode = 3;
        public AuthorizationException(int code, string message) : base(message)
        {
            Code = code;
        }


        public int Code { get; }

        internal static AuthorizationException NoAuthorization(string actionIdentifier)
        {
            return new AuthorizationException(NoAuthorizationCode, $"Authorization rules are missing for action {actionIdentifier}");
        }

        internal static AuthorizationException UnauthorizedHandler(IEnumerable<object> handlers)
        {
            var handlerNames = string.Join(", ", handlers.Select(h => h.GetType().FullName));
            return new AuthorizationException(UnauthorizedHandlerCode, $"All action handlers or no one have to provide authorization policies. These handlers did not have policies: [{handlerNames}]");
        }

        internal static AuthorizationException RuleNotMet(ICollection<Rule> allRules)
        {
            var notGrantedGroups = allRules
                .Where(r => !r.Granted)
                .GroupBy(r => r.Name, StringComparer.InvariantCultureIgnoreCase)
                .Select(g => $"{g.Key}: [{string.Join(", ", g)}]");
            var notGranted = $"[{string.Join(", ", notGrantedGroups)}]";

            var ex = new AuthorizationException(RuleNotMetCode, $"Policy rules {notGranted} not matched for current user.");
            var i = 1;
            foreach (var rule in allRules)
            {
                ex.Data[i] = rule;
                i++;
            }
            return ex;
        }
    }
}
