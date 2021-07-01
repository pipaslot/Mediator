using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Abstractions
{
    public class MediatorResponse<TResult> : MediatorResponse, IMediatorResponse<TResult>
    {
        /// <summary>
        /// Constructor for deserialization only
        /// </summary>
        public MediatorResponse()
        {
        }

        public MediatorResponse(string errorMessage) : base(errorMessage)
        {
        }

#pragma warning disable CS8603 // Possible null reference return.
        TResult IMediatorResponse<TResult>.Result => (TResult)Result;
#pragma warning restore CS8603 // Possible null reference return.

        TResult[] IMediatorResponse<TResult>.Results => Results
            .Select(r => (TResult)r)
            .ToArray();

        string[] IMediatorResponse<TResult>.ErrorMessages => ErrorMessages.ToArray();
    }

    public class MediatorResponse : IMediatorResponse
    {
        public MediatorResponse()
        {
        }

        public MediatorResponse(string errorMessage)
        {
            ErrorMessages.Add(errorMessage);
        }

        public bool Success => ErrorMessages.Count == 0;

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public string ErrorMessage => string.Join(";", ErrorMessages);
        public List<string> ErrorMessages { get; } = new List<string>();

        public object? Result => Results.FirstOrDefault();
        public List<object> Results { get; } = new List<object>(1);
    }
}