namespace Pipaslot.Mediator.Authorization.RuleSetFormatters
{
    /// <summary>
    /// Dependency injection extension for registering custom formatting strategy
    /// Rule set formating in case when exception is throw to stop unauthorized operation
    /// </summary>
    public interface IExceptionRuleSetFormatter : IRuleSetFormatter { }
}
