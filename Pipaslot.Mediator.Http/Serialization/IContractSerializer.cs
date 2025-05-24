using Pipaslot.Mediator.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Serialization;

public interface IContractSerializer
{
    SerializedRequest SerializeRequest(IMediatorAction request);
    Task<IMediatorAction> DeserializeRequest(Stream action, ICollection<StreamContract> dataStreams);
    string SerializeResponse(IMediatorResponse response);
    IMediatorResponse<TResult> DeserializeResponse<TResult>(string response);
}