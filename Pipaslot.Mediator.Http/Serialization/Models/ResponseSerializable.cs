namespace Pipaslot.Mediator.Http.Serialization.Models
{
    internal class ResponseSerializable
    {
        public bool Success { get; set; }
        public ContractSerializable[] Results { get; set; } = new ContractSerializable[0];
        public string[] ErrorMessages { get; set; } = new string[0];
    }
}
