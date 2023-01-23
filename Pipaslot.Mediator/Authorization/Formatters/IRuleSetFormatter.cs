using System.Collections.Generic;

namespace Pipaslot.Mediator.Authorization.Formatters
{
    public interface IRuleSetFormatter
    {
        Rule Format(Rule rule);
        Rule FormatDeniedWithAnd(ICollection<Rule> denied);
        Rule FormatDeniedWithOr(ICollection<Rule> denied);
    }
}