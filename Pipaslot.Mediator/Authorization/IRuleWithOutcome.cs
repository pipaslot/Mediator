using Pipaslot.Mediator.Authorization.Formatting;

namespace Pipaslot.Mediator.Authorization
{
    public interface IRuleWithOutcome : IRule
    {
        public RuleOutcome Outcome { get; }
    }
}
