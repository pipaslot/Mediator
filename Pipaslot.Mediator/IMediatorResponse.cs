namespace Pipaslot.Mediator
{
    public interface IMediatorResponse<TResult> : IMediatorResponse
    {
        /// <summary>
        /// Result from result set matching specified type
        /// </summary>
        TResult Result { get; }
        /// <summary>
        /// Results provided by middlewares and handlers
        /// </summary>
        object[] Results { get; }
    }

    public interface IMediatorResponse
    {
        bool Success { get; }
        /// <summary>
        /// Negated value of Success 
        /// </summary>
        bool Failure { get; }
        /// <summary>
        /// Concatenated error messages occured durign processing 
        /// </summary>
        string ErrorMessage { get; }
        /// <summary>
        /// Error messages occured durign processing 
        /// </summary>
        string[] ErrorMessages { get; }
    }
}
