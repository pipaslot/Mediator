﻿using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Http.Serialization;

public interface IContractSerializer
{
    string SerializeRequest(IMediatorAction request);
    IMediatorAction DeserializeRequest(string requestBody);
    string SerializeResponse(IMediatorResponse response);
    IMediatorResponse<TResult> DeserializeResponse<TResult>(string response);
}