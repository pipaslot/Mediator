using System.Linq;

namespace Pipaslot.Mediator.Authorization
{
    public class IsAuthorizedRequestResponse
    {
        public bool IsAuthorized => NotMetRule.Count() == 0;
        public Rule[] NotMetRule { get; }

        public IsAuthorizedRequestResponse(Rule[] notMetRule)
        {
            NotMetRule = notMetRule;
        }
    }
}
