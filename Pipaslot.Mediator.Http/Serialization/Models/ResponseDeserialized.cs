using System.Linq;

namespace Pipaslot.Mediator.Http.Serialization.Models
{
    internal class ResponseDeserialized
    {
        public bool Success { get; set; }
        public object[] Results { get; set; } = new object[0];
        public string[] ErrorMessages { get; set; } = new string[0];
    }
    internal class ResponseDeserialized<TResult> : IMediatorResponse<TResult>
    {
        public bool Success { get; set; }
        public bool Failure => !Success;
        public string ErrorMessage => string.Join(";", ErrorMessages);
        public TResult Result => (TResult)Results.FirstOrDefault(r => r is TResult);
        public object[] Results { get; set; } = new object[0];
        public string[] ErrorMessages { get; set; } = new string[0];
    }
}
