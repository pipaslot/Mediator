namespace Pipaslot.Mediator.Contracts
{
    /// <summary>
    /// Request contract with all necessary fields sent over network
    /// </summary>
    public class MediatorRequestSerializable
    {
        public const string Endpoint = "/_mediator/request";
        public const string VersionHeader = "MediatorAPIVersion";
        public const string VersionHeaderValueV1 = "";
        public const string VersionHeaderValueV2 = "v2";
        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string Json { get; set; } = string.Empty;

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
        public string ObjectName { get; set; } = string.Empty;
    }
}