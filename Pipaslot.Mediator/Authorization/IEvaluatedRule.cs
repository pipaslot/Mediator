using Pipaslot.Mediator.Authorization.Formatting;

namespace Pipaslot.Mediator.Authorization
{
    public interface IEvaluatedRule : IRule
    {
        public RuleOutcome Outcome { get; }
    }
}
