using Pipaslot.Mediator.Authorization.Formatting;

namespace Pipaslot.Mediator.Authorization
{
    /// <summary>
    /// Rule evaluated by the policy having outcome assigned.
    /// </summary>
    public interface IEvaluatedRule : IRule
    {
        public RuleOutcome Outcome { get; }
    }
}
