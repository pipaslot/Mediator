using System.Collections.Generic;

namespace Pipaslot.Mediator.Authorization
{
    public interface IRuleSet
    {
        bool Granted { get; }

        string StringifyNotGranted();

        public IEnumerable<Rule> Rules { get; }
    }
}
