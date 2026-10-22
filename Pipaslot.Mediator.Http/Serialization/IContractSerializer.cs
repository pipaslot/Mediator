using Pipaslot.Mediator.Abstractions;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Http.Serialization;

/// <summary>
/// Converts actions and responses to and from the wire contract exchanged between the client and the server.
/// </summary>
/// <remarks>
/// Decides only the shape of the payload; which types the deserializer may create is a separate decision made by
/// <see cref="Configuration.ICredibleProvider"/>, so a custom serializer must call it before instantiating a type named by the
/// payload. The client and the server must use the same wire format. The default implementation is registered by
/// <c>AddMediatorClient</c> and <c>AddMediatorServer</c>.
/// </remarks>
public interface IContractSerializer
{
    /// <summary>
    /// Serializes an action for sending to the server; streams held by the action are extracted into <see cref="SerializedRequest.Streams"/>.
    /// </summary>
    /// <param name="request">Action to send.</param>
    SerializedRequest SerializeRequest(IMediatorAction request);

    /// <summary>
    /// Creates the action from a received request.
    /// </summary>
    /// <param name="requestBody">JSON part of the request.</param>
    /// <param name="streams">Streams received together with the JSON, matched to the action's stream properties.</param>
    /// <exception cref="MediatorHttpException">The body is not a valid contract or names a type that is not credible.</exception>
    IMediatorAction DeserializeRequest(string requestBody, ICollection<StreamContract> streams);

    /// <summary>
    /// Serializes the response that the server sends back to the client.
    /// </summary>
    /// <param name="response">Response to send.</param>
    string SerializeResponse(IMediatorResponse response);

    /// <summary>
    /// Creates the response from the body received from the server.
    /// </summary>
    /// <param name="response">JSON body of the response.</param>
    /// <typeparam name="TResult">Result type the action declares.</typeparam>
    /// <exception cref="MediatorHttpException">The body is not a valid contract or names a type that is not credible.</exception>
    IMediatorResponse<TResult> DeserializeResponse<TResult>(string response);
}