using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Http
{
    /// <summary>
    /// Formats URL for Mediator calls over HTTP
    /// </summary>
    public interface IMediatorUrlFormatter
    {
        /// <summary>
        /// Format URL for HTTP GET request
        /// </summary>x
        string FormatHttpGet(IMediatorAction action);
    }
}
