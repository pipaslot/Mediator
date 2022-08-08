using System.Linq;

namespace Pipaslot.Mediator.Authorization
{
    public class IsAuthorizedRequestResponse
    {
        public bool IsAuthorized => NotMetRule.Count() == 0;
        public IRuleSet[] NotMetRule { get; }

        public IsAuthorizedRequestResponse(IRuleSet[] notMetRule)
        {
            NotMetRule = notMetRule;
        }
    }
}
