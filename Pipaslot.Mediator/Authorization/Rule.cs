using System.Threading.Tasks;
using System.Threading;
using System;

namespace Pipaslot.Mediator.Authorization
{
    public class Rule : IPolicy
    {
        /// <summary>
        /// Default rule name if not specified
        /// </summary>
        public const string DefaultName = "Rule";
        /// <summary>
        /// Name can be used also as kind like Authentication, Claim, Role or any custom name.
        /// The name is used by <see cref="IRuleSetFormatter"/> to distinguish
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Represents subject of the validation depending on the rule name. 
        /// It can contain name of role or claim required for the operation.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Flag saying that user can perform requrested operation.
        /// </summary>
        public bool Granted { get; }

        /// <summary>
        /// Define application rule using default name <see cref="DefaultName"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="granted"></param>
        public Rule(string value, bool granted = false) : this(DefaultName, value, granted)
        {

        }

        public Rule(string name, string value, bool granted = false)
        {
            Name = name;
            Value = value;
            Granted = granted;
        }

        public Task<RuleSet> Resolve(IServiceProvider services, CancellationToken cancellationToken)
        {
            var set = new RuleSet(this);
            return Task.FromResult(set);
        }
    }
}
