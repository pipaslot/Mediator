using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator
{
    public class MediatorResponse<TResult> : MediatorResponse, IMediatorResponse<TResult>
    {
        [Obsolete()]
        /// <summary>
        /// Constructor for deserialization only
        /// </summary>
        public MediatorResponse()
        {
        }

        public MediatorResponse(string errorMessage) : base(errorMessage)
        {
        }


        [Obsolete()]
        public MediatorResponse(IEnumerable<object> results, IEnumerable<string> errorMessages) : base(errorMessages.Count() == 0 && results.Any(r => r is TResult), results, errorMessages)
        {
        }

        public MediatorResponse(bool success, IEnumerable<object> results, IEnumerable<string> errorMessages) : base(success, results, errorMessages)
        {
        }

        TResult IMediatorResponse<TResult>.Result => (TResult)Results.FirstOrDefault(r => r is TResult);

        object[] IMediatorResponse<TResult>.Results => Results.ToArray();

        string[] IMediatorResponse<TResult>.ErrorMessages => ErrorMessages.ToArray();
    }

    public class MediatorResponse : IMediatorResponse
    {
        [Obsolete()]
        public MediatorResponse()
        {
        }

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

        [Obsolete()]
        public MediatorResponse(IEnumerable<object> results, IEnumerable<string> errorMessages)
        {
            Results.AddRange(results);
            ErrorMessages.AddRange(errorMessages);
            Success = errorMessages.Count() == 0;
        }

        public bool Success { get; }

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public string ErrorMessage => string.Join(";", ErrorMessages);
        public List<string> ErrorMessages { get; } = new List<string>();

        public object? Result => Results.FirstOrDefault();
        public List<object> Results { get; } = new List<object>(1);
    }
}