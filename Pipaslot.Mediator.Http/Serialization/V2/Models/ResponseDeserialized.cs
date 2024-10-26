namespace Pipaslot.Mediator.Http.Serialization.V2.Models;

internal class ResponseDeserialized
{
    public bool Success { get; set; }
    public object[] Results { get; set; } = [];
}

internal class ResponseDeserialized<TResult> : IMediatorResponse<TResult>
{
    public bool Success { get; set; }
    public bool Failure => !Success;
    public TResult Result => this.GetResult<TResult>();
    public object[] Results { get; set; } = [];
}
