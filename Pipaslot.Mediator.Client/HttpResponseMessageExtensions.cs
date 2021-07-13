using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Client
{
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// Parse mediator response as specified result type
        /// </summary>
        /// <typeparam name="TResult">Expected deserialized result type</typeparam>
        /// <param name="response"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<IMediatorResponse<TResult>> ParseResponse<TResult>(this HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            return await response.Content.ReadFromJsonAsync<MediatorResponseDeserialized<TResult>>(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Parse mediator response as specified result type
        /// </summary>
        /// <typeparam name="TResult">Expected deserialized result type</typeparam>
        /// <param name="response"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Response is returned only if Success == true, otherwise will be null</returns>
        public static async Task<(bool Success, IMediatorResponse<TResult> Response)> TryParseResponse<TResult>(this HttpResponseMessage response, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await ParseResponse<TResult>(response, cancellationToken);
                return (true, result);
            }
            catch
            {
                // Do nothing
            }
            return (false, null);
        }
    }
}
