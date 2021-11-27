namespace Pipaslot.Mediator.Http.Contracts
{
    /// <summary>
    /// Request contract with all necessary fields sent over network
    /// </summary>
    internal class MediatorRequestSerializable
    {        
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string Json { get; set; } = string.Empty;

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string ObjectName { get; set; } = string.Empty;
    }
}