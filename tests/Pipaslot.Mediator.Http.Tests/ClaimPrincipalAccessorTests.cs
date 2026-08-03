using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace Pipaslot.Mediator.Http.Tests;

/// <summary>
/// <see cref="ClaimPrincipalAccessor"/> is a one-line delegator onto <see cref="IHttpContextAccessor"/>; it is
/// constructed nowhere else in this project, so its two branches (context present/absent) had no direct coverage.
/// </summary>
public class ClaimPrincipalAccessorTests
{
    [Fact]
    public void Principal_HttpContextHasUser_ReturnsUserFromHttpContext()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(c => c.User).Returns(principal);
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns(httpContext.Object);
        var sut = new ClaimPrincipalAccessor(accessor.Object);

        var result = sut.Principal;

        Assert.Same(principal, result);
    }

    [Fact]
    public void Principal_HttpContextIsNull_ReturnsNull()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(a => a.HttpContext).Returns((HttpContext)null!);
        var sut = new ClaimPrincipalAccessor(accessor.Object);

        var result = sut.Principal;

        Assert.Null(result);
    }
}
