namespace Pipaslot.Mediator.Http.Contracts
{
    /// <summary>
    /// Response contract with all necessary fields sent over network
    /// </summary>
    internal class MediatorResponseSerializable
    {
        public bool Success { get; set; }
        public SerializedResult[] Results { get; set; } = new SerializedResult[0];
        public string[] ErrorMessages { get; set; } = new string[0];

        public class SerializedResult
        {
            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
            public string Json { get; set; } = string.Empty;

            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
            public string ObjectName { get; set; } = string.Empty;
        }
    }
}