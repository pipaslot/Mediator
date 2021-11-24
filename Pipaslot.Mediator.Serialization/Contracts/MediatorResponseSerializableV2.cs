namespace Pipaslot.Mediator.Serialization
{
    /// <summary>
    /// Response contract with all necessary fields sent over network
    /// </summary>
    public class MediatorResponseSerializableV2
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