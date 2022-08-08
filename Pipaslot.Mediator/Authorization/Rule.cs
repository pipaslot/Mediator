namespace Pipaslot.Mediator.Authorization
{
    /// <summary>
    /// Define rule produced by the resolved policy.
    /// </summary>
    public class Rule
    {
        public string Name { get; }
        public string Value { get; }
        public bool Granted { get; }

        public Rule(string name, string value, bool granted = false)
        {
            Name = name;
            Value = value;
            Granted = granted;
        }
    }
}
