namespace Pipaslot.Mediator.Abstractions
{
    public interface IMediatorResponse<TResult> : IMediatorResponse
    {
        TResult Result { get; }
        TResult[] Results { get; }
        string[] ErrorMessages { get; }
    }

    public interface IMediatorResponse
    {
        bool Success { get; }
        string ErrorMessage { get; }
    }
}
