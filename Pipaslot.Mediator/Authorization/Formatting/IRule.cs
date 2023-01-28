namespace Pipaslot.Mediator.Authorization.Formatting
{
    public interface IRule
    {
        /// <summary>
        /// Name can be used also as kind like Authentication, Claim, Role or any custom name.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Represents subject of the validation depending on the rule name. 
        /// It can contain name of role or claim required for the operation.
        /// </summary>
        public string Value { get; }
    }
}
