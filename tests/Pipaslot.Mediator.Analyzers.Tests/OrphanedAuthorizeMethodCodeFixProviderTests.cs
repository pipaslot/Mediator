using System.Threading.Tasks;

namespace Pipaslot.Mediator.Analyzers.Tests;

using VerifyCS = CSharpCodeFixVerifier<OrphanedAuthorizeMethodAnalyzer, OrphanedAuthorizeMethodCodeFixProvider>;

/// <summary>
/// Covers <see cref="OrphanedAuthorizeMethodCodeFixProvider"/>'s single fix for
/// <see cref="DiagnosticDescriptors.OrphanedAuthorizeMethodId"/>: adding the matching authorization interface to
/// the flagged type's base list, using the flagged method's own parameter type as the interface's type argument.
/// </summary>
public class OrphanedAuthorizeMethodCodeFixProviderTests
{
    private const string Preamble = """
        using System.Threading;
        using System.Threading.Tasks;
        using Pipaslot.Mediator;
        using Pipaslot.Mediator.Authorization;

        public class CreateOrder : IMessage { }

        """;

    [Fact]
    public async Task Authorize_AddsHandlerAuthorizationInterfaceToBaseList()
    {
        var source = Preamble + """
            public class CreateOrderHandler : IMessageHandler<CreateOrder>
            {
                public IPolicy {|#0:Authorize|}(CreateOrder action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;
        var fixedSource = Preamble + """
            public class CreateOrderHandler : IMessageHandler<CreateOrder>, IHandlerAuthorization<CreateOrder>
            {
                public IPolicy Authorize(CreateOrder action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.OrphanedAuthorizeMethodId)
            .WithLocation(0)
            .WithArguments("CreateOrderHandler", "Authorize", "CreateOrder", "IHandlerAuthorization");

        await VerifyCS.VerifyCodeFixAsync(source, expected, fixedSource);
    }

    [Fact]
    public async Task AuthorizeAsync_AddsHandlerAuthorizationAsyncInterfaceToBaseList()
    {
        var source = Preamble + """
            public class CreateOrderHandler : IMessageHandler<CreateOrder>
            {
                public Task<IPolicy> {|#0:AuthorizeAsync|}(CreateOrder action, CancellationToken cancellationToken)
                    => Task.FromResult<IPolicy>(IdentityPolicy.Authenticated());

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;
        var fixedSource = Preamble + """
            public class CreateOrderHandler : IMessageHandler<CreateOrder>, IHandlerAuthorizationAsync<CreateOrder>
            {
                public Task<IPolicy> AuthorizeAsync(CreateOrder action, CancellationToken cancellationToken)
                    => Task.FromResult<IPolicy>(IdentityPolicy.Authenticated());

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.OrphanedAuthorizeMethodId)
            .WithLocation(0)
            .WithArguments("CreateOrderHandler", "AuthorizeAsync", "CreateOrder", "IHandlerAuthorizationAsync");

        await VerifyCS.VerifyCodeFixAsync(source, expected, fixedSource);
    }
}
