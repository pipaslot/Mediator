using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator
{
    public class MediatorResponse<TResult> : MediatorResponse, IMediatorResponse<TResult>
    {
        public MediatorResponse(string errorMessage) : base(errorMessage)
        {
        }

        public MediatorResponse(bool success, IEnumerable<object> results, IEnumerable<string> errorMessages) : base(success, results, errorMessages)
        {
        }

        TResult IMediatorResponse<TResult>.Result => (TResult)Results.FirstOrDefault(r => r is TResult);

        object[] IMediatorResponse<TResult>.Results => Results.ToArray();        
    }

    public class MediatorResponse : IMediatorResponse
    {
        public MediatorResponse(string errorMessage)
        {
            ErrorMessages.Add(errorMessage);
            Success = false;
        }
        public MediatorResponse(bool success, IEnumerable<object> results, IEnumerable<string> errorMessages)
        {
            Results.AddRange(results);
            ErrorMessages.AddRange(errorMessages);
            Success = success;
        }

        public bool Success { get; }
        public bool Failure => !Success;

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public string ErrorMessage => string.Join(";", ErrorMessages);
        public List<string> ErrorMessages { get; } = new List<string>();
        string[] IMediatorResponse.ErrorMessages => ErrorMessages.ToArray();

        public object? Result => Results.FirstOrDefault();
        public List<object> Results { get; } = new List<object>(1);

    }
}