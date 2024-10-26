namespace Pipaslot.Mediator.Http.Configuration;

/// <summary>
/// Options configured and availabl as singleton instance
/// </summary>
public interface IMediatorOptions
{
    /// <summary>
    /// Configure the serializer to ignore read only properties. 
    /// This may break deserialization for objects with constructor initializing the properties via private setters.
    /// Is supported only by V3 serializer
    /// </summary>
    public bool IgnoreReadOnlyProperties { get; }
}