namespace Pipaslot.Mediator.Authorization
{
    public class Rule
    {
        public string Name { get; }
        public string Value { get; }
        public bool Granted { get; }

        public Rule(string name, string value, bool granted)
        {
            Name = name;
            Value = value;
            Granted = granted;
        }
    }
}
