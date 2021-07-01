namespace Pipaslot.Mediator.Abstractions
{
    /// <summary>
    /// Response contract with all necessary fields sent over network
    /// </summary>
    public class MediatorResponseSerializable
    {
        public bool Success { get; set; }
        public object[] Results { get; set; } = new object[0];
        public string[] ErrorMessages { get; set; } = new string[0];
    }
}