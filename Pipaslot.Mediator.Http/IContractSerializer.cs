namespace Pipaslot.Mediator.Http
{
    public interface IContractSerializer
    {
        string SerializeRequest(object request, out string actionName);
        MediatorRequestDeserialized DeserializeRequest(string requestBody);
        string SerializeResponse(IMediatorResponse response);
        IMediatorResponse<TResult> DeserializeResponse<TResult>(string response);
    }
}
