using Pipaslot.Mediator.Abstractions;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Http.Serialization;

public interface IContractSerializer
{
    SerializedRequest SerializeRequest(IMediatorAction request);
    IMediatorAction DeserializeRequest(string requestBody, ICollection<StreamContract> streams);
    string SerializeResponse(IMediatorResponse response);
    IMediatorResponse<TResult> DeserializeResponse<TResult>(string response);
}