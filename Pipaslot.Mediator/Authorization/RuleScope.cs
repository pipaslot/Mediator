namespace Pipaslot.Mediator.Authorization
{
    public enum RuleScope
    {
        /// <summary>
        /// Rule outcome is changing during the time and may be caluculated from model state.
        /// </summary>
        State = 0,
        /// <summary>
        /// Rule outcope wont change during user session. Can be applied caching techniques.
        /// </summary>
        Identity = 1,
    }
}
