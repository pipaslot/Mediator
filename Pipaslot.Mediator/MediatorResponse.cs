﻿using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator
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
        public MediatorResponse(IEnumerable<object> results, IEnumerable<string> errorMessages)
        {
            Results.AddRange(results);
            ErrorMessages.AddRange(errorMessages);
        }

        TResult IMediatorResponse<TResult>.Result => (TResult)Results.FirstOrDefault(r =>r is TResult);

        object[] IMediatorResponse<TResult>.Results => Results.ToArray();

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

        public MediatorResponse(IEnumerable<object> results, IEnumerable<string> errorMessages)
        {
            Results.AddRange(results);
            ErrorMessages.AddRange(errorMessages);
        }

        public bool Success => ErrorMessages.Count == 0;

        // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Local
        public string ErrorMessage => string.Join(";", ErrorMessages);
        public List<string> ErrorMessages { get; } = new List<string>();

        public object? Result => Results.FirstOrDefault();
        public List<object> Results { get; } = new List<object>(1);
    }
}