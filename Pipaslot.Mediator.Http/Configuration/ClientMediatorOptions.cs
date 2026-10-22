namespace Pipaslot.Mediator.Http.Configuration;

/// <summary>
/// Options of the client side of the HTTP transport, passed to
/// <see cref="ServiceCollectionExtensions.AddMediatorClient(Microsoft.Extensions.DependencyInjection.IServiceCollection, System.Action{ClientMediatorOptions})"/>.
/// </summary>
/// <remarks>
/// The client sends actions to the server and deserializes the results it gets back, so the credible-type allowlist
/// configured here applies to result types (see <see cref="BaseMediatorOptions{TBuilder}.DeserializeOnlyCredibleResultTypes"/>).
/// The server counterpart is <see cref="ServerMediatorOptions"/>.
/// </remarks>
public class ClientMediatorOptions : BaseMediatorOptions<ClientMediatorOptions>
{
    /// <inheritdoc/>
    public override bool AddContextAccessor { get; set; } = false;
}