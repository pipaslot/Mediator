namespace Pipaslot.Mediator.Authorization;

public static class PolicyExtensions
{
    /// <summary>
    /// Combine multiple policies together with AND operator
    /// </summary>
    /// <returns>New policy instance</returns>
    public static Policy And(this IPolicy policy, params IPolicy[] andPolicies)
    {
        var expression = new Policy(Operator.And) { policy };
        expression.AddRange(andPolicies);
        return expression;
    }

    /// <summary>
    /// Combine multiple policies together with OR operator
    /// </summary>
    /// <returns>New policy instance</returns>
    public static Policy Or(this IPolicy policy, params IPolicy[] orPolicies)
    {
        var expression = new Policy(Operator.Or) { policy };
        expression.AddRange(orPolicies);
        return expression;
    }
}