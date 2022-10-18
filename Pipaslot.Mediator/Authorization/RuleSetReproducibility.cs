namespace Pipaslot.Mediator.Authorization
{
    public enum RuleSetReproducibility
    {
        /// <summary>
        /// The rule is always computed from incoming data. Or the rules in the rule set may vary regarding current state.
        /// Caching shouldnt be applied.
        /// </summary>
        Dynamic = 0,

        /// <summary>
        /// The rule wont change until the user identity is the same. The
        /// Caching has to consider indentity change
        /// </summary>
        IdentityStatic = 1,
    }
}
