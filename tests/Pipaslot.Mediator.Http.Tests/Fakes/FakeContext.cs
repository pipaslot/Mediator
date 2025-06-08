using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;

namespace Pipaslot.Mediator.Http.Tests.Fakes;

internal class FakeContext : HttpContext
{
    private readonly FeatureCollection _features = new();

    public FakeContext(HttpRequest request, IServiceProvider services, HttpResponse? response = null)
    {
        Request = request;
        Response = response ?? new FakeResponse();
        RequestServices = services;
    }

    public override IFeatureCollection Features => _features;

    public override HttpRequest Request { get; }

    public override HttpResponse Response { get; }

    public override ConnectionInfo Connection => throw new NotImplementedException();

    public override WebSocketManager WebSockets => throw new NotImplementedException();

    [Obsolete]
    public override AuthenticationManager Authentication => throw new NotImplementedException();

    public override ClaimsPrincipal User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override IDictionary<object, object> Items
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }

    public override IServiceProvider RequestServices { get; set; }
    public override CancellationToken RequestAborted { get; set; } = CancellationToken.None;
    public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public override void Abort()
    {
    }
}