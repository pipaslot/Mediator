using Pipaslot.Mediator.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator
{
    public static class MediatorExtensions
    {
        /// <summary>
        /// Execute action and return result if operation succeded otherwise returns default value for the result type
        /// </summary>
        /// <typeparam name="TResult">Action result type</typeparam>
        /// <returns>Action result type of its defaut implementation</returns>
        public static async Task<TResult?> ExecuteOrDefault<TResult>(this IMediator mediator ,IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
        {
            var response = await mediator.Execute(request, cancellationToken);
            return response.Success ? response.Result : default;
        }
    }
}
