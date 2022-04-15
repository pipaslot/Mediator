namespace Pipaslot.Mediator.Http.Serialization.Models
{
    internal class ContractSerializable
    {
        public ContractSerializable(object content, string type)
        {
            Content = content;
            Type = type;
        }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public object Content { get; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string Type { get; }
    }
}
