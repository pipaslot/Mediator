using System.Linq;

namespace Pipaslot.Mediator.Client
{
    public class MediatorResponseDeserialized<TResult> : IMediatorResponse<TResult>
    {
        public bool Success { get; set; }
        public bool Failure => !Success;
        public string ErrorMessage => string.Join(";", ErrorMessages);
        public TResult Result => (TResult)Results.FirstOrDefault(r => r is TResult);
        public object[] Results { get; set; }
        public string[] ErrorMessages { get; set; } = new string[0];
    }
}
