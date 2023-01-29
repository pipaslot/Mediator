namespace Pipaslot.Mediator.Authorization.Formatting
{
    public class EvaluatedRule : IEvaluatedRule
    {
        private IRule _pair;

        public EvaluatedRule(IRule pair, RuleOutcome outcome)
        {
            _pair = pair;
            Outcome = outcome;
        }

        public string Name => _pair.Name;
        public string Value => _pair.Value;
        public RuleOutcome Outcome { get; }
    }
}
