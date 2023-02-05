using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.VisualBasic;

namespace Pipaslot.Mediator.Authorization
{
    /// <summary>
    /// Rule and RuleImpact can lead in three different states: Unavailable, Deny, Allow
    /// Default state is Deny. This state is applied also when no rule was defined.
    /// Unavailable state can occure when
    /// </summary>
    public class Rule : IPolicy, IEvaluatedRule
    {
        public RuleScope Scope { get; } = RuleScope.State;

        public RuleOutcome Outcome { get; } = RuleOutcome.Deny;

        /// <summary>
        /// Default rule name if not specified. It is used in cases where the value should serve as a sentence or when we want to prevent additional formatting.
        /// </summary>
        public const string DefaultName = "RuleWithReasoning";
        /// <summary>
        /// Rule name used in situation when multiple rules were joined by operator to be later detected by the formatter
        /// </summary>
        public const string JoinedFormatedRuleName = "JoinedFormatedRuleName";
        /// <summary>
        /// Name can be used also as kind like Authentication, Claim, Role or any custom name.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Represents subject of the validation depending on the rule name. 
        /// It can contain name of role or claim required for the operation.
        /// </summary>
        public string Value { get; }

        internal Rule(RuleOutcome outcome, string value) : this(DefaultName, value, outcome)
        {

        }

        public Rule(string name, string value, RuleOutcome outcome = RuleOutcome.Deny, RuleScope scope = RuleScope.State)
        {
            Name = name;
            Value = value;
            Scope = scope;
            Outcome = outcome;
        }

        public Rule(string name, string value, bool granted, RuleScope scope = RuleScope.State)
        {
            Name = name;
            Value = value;
            Scope = scope;
            Outcome = granted ? RuleOutcome.Allow : RuleOutcome.Deny;
        }

        public Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
        {
            var set = new RuleSet(this);
            return Task.FromResult(set);
        }

        /// <summary>
        /// Create a rule/policy depending on the state of the application and provide feedback to the user
        /// </summary>
        /// <returns>Rule with Allow outcome</returns>
        public static Rule Allow(string reason = "")
        {
            return new Rule(RuleOutcome.Allow, reason);
        }

        /// <summary>
        /// Create a rule/policy depending on the state of the application and provide feedback to the user
        /// </summary>
        /// <returns>Rule with Deny outcome</returns>
        public static Rule Deny(string reason = "")
        {
            return new Rule(RuleOutcome.Deny, reason);
        }

        /// <summary>
        /// Create a rule/policy depending on the state of the application and provide feedback to the user
        /// </summary>
        /// <returns>Rule with Undefined outcome</returns>
        public static Rule Unavailable(string reason = "")
        {
            return new Rule(RuleOutcome.Unavailable, reason);
        }

        /// <summary>
        /// Create a rule/policy depending on the state of the application and provide feedback to the user
        /// </summary>
        /// <param name="condition">Condition</param>
        /// <param name="reason">Reason applied when condition is TRUE</param>
        /// <returns>Returns Allow if the condition is true, otherwise returns Ignored</returns>
        public static Rule AllowIf(bool condition, string reason = "")
        {
            return new Rule(
                condition ? RuleOutcome.Allow : RuleOutcome.Ignored,
                condition ? reason : string.Empty);
        }

        /// <summary>
        /// Create a rule/policy depending on the state of the application and provide feedback to the user
        /// </summary>
        /// <param name="condition">Condition</param>
        /// <param name="reason">Reason applied when condition is TRUE</param>
        /// <returns>Returns Deny if the condition is true, otherwise returns Ignored</returns>
        public static Rule DenyIf(bool condition, string reason = "")
        {
            return new Rule(
                condition ? RuleOutcome.Deny : RuleOutcome.Ignored,
                condition ? reason : string.Empty);
        }

        /// <summary>
        /// Create Rule/Policy depending on application state
        /// </summary>
        /// <param name="condition">Condition</param>
        /// <param name="reason">Reason applied when condition is TRUE</param>
        /// <returns>Returns Unavailable if the condition is true, otherwise returns Ignored</returns>
        public static Rule UnavailableIf(bool condition, string reason = "")
        {
            return new Rule(
                condition ? RuleOutcome.Unavailable : RuleOutcome.Ignored,
                condition ? reason : string.Empty);
        }

        /// <summary>
        /// Create a rule/policy depending on the state of the application and provide feedback to the user
        /// </summary>
        /// <param name="condition">Condition</param>
        /// <param name="allowReason">Reason applied when condition is TRUE</param>
        /// <param name="denyReason">Reason applied when condition is FALSE</param>
        /// <returns>Returns Allow if the condition is true, otherwise returns Deny</returns>
        public static Rule AllowOrDeny(bool condition, string allowReason = "", string denyReason = "")
        {
            return new Rule(
                condition ? RuleOutcome.Allow : RuleOutcome.Deny,
                condition ? allowReason : denyReason);
        }

        /// <summary>
        /// Create a rule/policy depending on the state of the application and provide feedback to the user
        /// </summary>
        /// <param name="condition">Condition</param>
        /// <param name="denyReason">Reason applied when condition is TRUE</param>
        /// <param name="allowReason">Reason applied when condition is FALSE</param>
        /// <returns>Returns Deny if the condition is true, otherwise returns Allow</returns>
        public static Rule DenyOrAllow(bool condition, string denyReason = "", string allowReason = "")
        {
            return new Rule(
                condition ? RuleOutcome.Deny : RuleOutcome.Allow,
                condition ? denyReason : allowReason);
        }

        /// <summary>
        /// Create a rule/policy depending on the state of the application and provide feedback to the user
        /// </summary>
        /// <param name="condition">Condition</param>
        /// <param name="denyReason">Reason applied when condition is FALSE</param>
        /// <param name="allowReason">Reason applied when condition is TRUE</param>
        /// <returns>Rule with Allow or Deny outcome</returns>
        [Obsolete]
        public static Rule Allow(bool condition, string denyReason = "", string allowReason = "")
        {
            return new Rule(
                condition ? RuleOutcome.Allow : RuleOutcome.Deny,
                condition ? allowReason : denyReason);
        }

        /// <summary>
        /// Create a rule/policy depending on the state of the application and provide feedback to the user
        /// </summary>
        /// <param name="condition">Condition</param>
        /// <param name="denyReason">Reason applied when condition is TRUE</param>
        /// <returns>Rule with Deny or ignored outcome</returns>
        [Obsolete]
        public static Rule Deny(bool condition, string denyReason = "")
        {
            return new Rule(
                condition ? RuleOutcome.Deny : RuleOutcome.Ignored,
                condition ? denyReason : string.Empty);
        }

        /// <summary>
        /// Create Rule/Policy depending on application state
        /// </summary>
        /// <param name="condition">Condition</param>
        /// <returns>Return Unavailable outcome if the condition is true, otherwise returns Ignored outcome</returns>
        [Obsolete]
        public static Rule Unavailable(bool condition)
        {
            return new Rule(condition ? RuleOutcome.Unavailable : RuleOutcome.Ignored, string.Empty);
        }

#if !NETSTANDARD
        public static RuleSet operator +(Rule rule1, Rule rule2)
        {
            return new RuleSet(Operator.Add, new[] { rule1, rule2 });
        }

        public static RuleSet operator |(Rule rule1, Rule rule2)
        {
            return new RuleSet(Operator.Or, new[] { rule1, rule2 });
        }
#endif
    }
}
