﻿using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Http.Serialization
{
    public interface IContractSerializer
    {
        //TODO change object request by IMediatorAction
        string SerializeRequest(object request);
        IMediatorAction DeserializeRequest(string requestBody);
        string SerializeResponse(IMediatorResponse response);
        IMediatorResponse<TResult> DeserializeResponse<TResult>(string response);
    }
}
