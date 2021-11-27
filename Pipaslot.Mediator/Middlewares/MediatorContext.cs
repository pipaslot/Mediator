﻿using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Middlewares
{
    public class MediatorContext
    {
        /// <summary>
        /// Handler error message and error messages colelcted during middleware processing
        /// </summary>
        public List<string> ErrorMessages { get; } = new List<string>();

        /// <summary>
        /// Handler result objects and object collected during middleware processing
        /// </summary>
        public List<object> Results { get; } = new List<object>(1);

        internal string[] ErrorMessagesDistincted => ErrorMessages.Distinct().ToArray();

        public MediatorContext CopyEmpty()
        {
            var copy = new MediatorContext();
            return copy;
        }

        /// <summary>
        /// Append properties from context
        /// </summary>
        /// <param name="context"></param>
        public void Append(MediatorContext context)
        {
            ErrorMessages.AddRange(context.ErrorMessages);
            Results.AddRange(context.Results);
        }

        /// <summary>
        /// Append properties from response
        /// </summary>
        /// <param name="response"></param>
        public void Append(IMediatorResponse response)
        {
            ErrorMessages.AddRange(response.ErrorMessages);
            Results.AddRange(response.Results);
        }
    }
}
