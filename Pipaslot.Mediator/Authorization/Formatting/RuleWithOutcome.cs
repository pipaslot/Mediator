namespace Pipaslot.Mediator.Authorization.Formatting
{
    public class RuleWithOutcome : IRuleWithOutcome
    {
        private IRule pair;

        public RuleWithOutcome(IRule pair, RuleOutcome outcome)
        {
            this.pair = pair;
            Outcome = outcome;
        }

        public string Name => pair.Name;
        public string Value => pair.Value;
        public RuleOutcome Outcome { get; }
    }
}
