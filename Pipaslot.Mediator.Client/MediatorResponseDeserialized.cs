using Pipaslot.Mediator.Abstractions;
using System.Linq;

namespace Pipaslot.Mediator.Client
{
    public class MediatorResponseDeserialized<TResult> : IMediatorResponse<TResult>
    {
        public bool Success => ErrorMessages.Count() == 0;
        public string ErrorMessage => string.Join(";", ErrorMessages);
        public TResult Result => Results.FirstOrDefault();
        public TResult[] Results { get; set; }
        public string[] ErrorMessages { get; set; } = new string[0];
    }
}
