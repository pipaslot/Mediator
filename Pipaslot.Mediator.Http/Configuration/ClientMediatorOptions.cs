namespace Pipaslot.Mediator.Http.Configuration;

public class ClientMediatorOptions : BaseMediatorOptions<ClientMediatorOptions>
{
    /// <inheritdoc/>
    public override bool AddContextAccessor { get; set; } = false;
}