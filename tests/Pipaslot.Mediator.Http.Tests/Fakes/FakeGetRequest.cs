using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pipaslot.Mediator.Http.Tests.Fakes;

internal class FakeGetRequest : FakePostRequest
{
    public override string Method { get; set; } = "GET";
    public override IQueryCollection Query { get; set; }
    public override Stream Body { get; set; } = new Mock<Stream>().Object;

    public FakeGetRequest(string action) : base("")
    {
        var query = new QueryCollection();
        query.Add(new KeyValuePair<string, StringValues>("action",
            new StringValues(action)
        ));
        Query = query;
    }
#pragma warning disable CS8644 // Type does not implement interface member. Nullability of reference types in interface implemented by the base type doesn't match.
    private class QueryCollection : List<KeyValuePair<string, StringValues>>, IQueryCollection
#pragma warning restore CS8644 // Type does not implement interface member. Nullability of reference types in interface implemented by the base type doesn't match.
    {
        public StringValues this[string key] => this.FirstOrDefault(v => v.Key == key).Value;

        public ICollection<string> Keys => throw new NotImplementedException();

        public bool ContainsKey(string key)
        {
            return this.Any(v => v.Key == key);
        }

        public bool TryGetValue(string key, out StringValues value)
        {
            var a = this.FirstOrDefault(v => v.Key == key);
            if (string.IsNullOrEmpty(a.Key))
            {
                value = default;
                return false;
            }

            value = a.Value;
            return true;
        }
    }
}