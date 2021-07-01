namespace Pipaslot.Mediator.Abstractions
{
    /// <summary>
    /// Request contract with all necessary fields sent over network
    /// </summary>
    public class MediatorRequestSerializable
    {
        public const string Endpoint = "/_mediator/request";
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string Json { get; set; } = string.Empty;
        
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string ObjectName { get; set; } = string.Empty;
    }
}