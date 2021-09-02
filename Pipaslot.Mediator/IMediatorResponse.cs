namespace Pipaslot.Mediator
{
    public interface IMediatorResponse<TResult> : IMediatorResponse
    {
        TResult Result { get; }
        /// <summary>
        /// Results provided by middlewares and handlers
        /// </summary>
        object[] Results { get; }
        string[] ErrorMessages { get; }
    }

    public interface IMediatorResponse
    {
        bool Success { get; }
        /// <summary>
        /// Negated value of Success 
        /// </summary>
        bool Failure { get; }
        string ErrorMessage { get; }
    }
}
