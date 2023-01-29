using System;
using System.Linq;

namespace Pipaslot.Mediator.Http.Serialization.V2.Models
{
    internal class ResponseDeserialized
    {
        public bool Success { get; set; }
        public object[] Results { get; set; } = Array.Empty<object>();
    }
    internal class ResponseDeserialized<TResult> : IMediatorResponse<TResult>
    {
        public bool Success { get; set; }
        public bool Failure => !Success;
        public TResult Result => (TResult)Results.FirstOrDefault(r => r is TResult)!;
        public object[] Results { get; set; } = Array.Empty<object>();
    }
}
