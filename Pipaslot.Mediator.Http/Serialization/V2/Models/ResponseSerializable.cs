namespace Pipaslot.Mediator.Http.Serialization.V2.Models;

internal class ResponseSerializable
{
    public bool Success { get; set; }
    public ContractSerializable[] Results { get; set; } = [];
}