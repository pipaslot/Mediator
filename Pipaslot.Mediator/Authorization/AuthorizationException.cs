using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Authorization
{
    public class AuthorizationException : Exception
    {
        public AuthorizationException(AuthorizationExceptionTypes type, string message) : base(message)
        {
            Type = type;
        }


        public AuthorizationExceptionTypes Type { get; }


        internal static AuthorizationException NoAuthorization(string actionIdentifier)
        {
            return new AuthorizationException(AuthorizationExceptionTypes.NoAuthorization, $"Authorization policies are missing for action {actionIdentifier}");
        }

        internal static AuthorizationException UnauthorizedHandler(IEnumerable<object> handlers)
        {
            var handlerNames = string.Join(", ", handlers.Select(h => h.GetType().FullName));
            return new AuthorizationException(AuthorizationExceptionTypes.UnauthorizedHandler, $"All action handlers or no one have to provide authorization policies. These handlers did not have policies: [{handlerNames}]");
        }
    }
}
