using System.Threading.Tasks;

namespace Pipaslot.Mediator.Analyzers.Tests;

using VerifyCS = CSharpAnalyzerVerifier<OrphanedAuthorizeMethodAnalyzer>;

/// <summary>
/// Covers <see cref="OrphanedAuthorizeMethodAnalyzer"/>: it must report a public <c>Authorize</c>/<c>AuthorizeAsync</c>
/// method whose parameter matches (or is a base of) an action the handler handles, whose return type is exactly
/// <c>IPolicy</c>/<c>Task&lt;IPolicy&gt;</c>, but whose declaring type does not implement
/// <c>IHandlerAuthorization&lt;T&gt;</c>/<c>IHandlerAuthorizationAsync&lt;T&gt;</c>. It must not report when the type
/// already implements an authorization interface (PIPMED003's territory instead), when the method is referenced from
/// elsewhere inside the type, when the parameter/return type doesn't match the interface shape exactly, or when the
/// type isn't a handler at all.
/// </summary>
public class OrphanedAuthorizeMethodAnalyzerTests
{
    [Fact]
    public async Task Handler_DeclaresMatchingAuthorizeMethodWithoutInterface_ReportsDiagnostic()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public class CreateOrderHandler : IMessageHandler<CreateOrder>
            {
                public IPolicy {|#0:Authorize|}(CreateOrder action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;
        
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.OrphanedAuthorizeMethodId)
            .WithLocation(0)
            .WithArguments("CreateOrderHandler", "Authorize", "CreateOrder", "IHandlerAuthorization");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Handler_DeclaresMatchingAuthorizeAsyncMethodWithoutInterface_ReportsDiagnostic()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public class CreateOrderHandler : IMessageHandler<CreateOrder>
            {
                public Task<IPolicy> {|#0:AuthorizeAsync|}(CreateOrder action, CancellationToken cancellationToken)
                    => Task.FromResult<IPolicy>(IdentityPolicy.Authenticated());

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.OrphanedAuthorizeMethodId)
            .WithLocation(0)
            .WithArguments("CreateOrderHandler", "AuthorizeAsync", "CreateOrder", "IHandlerAuthorizationAsync");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Handler_AuthorizesSharedBaseTypeOfHandledActions_ReportsDiagnostic()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public abstract class OrderAction : IMessage { }

            public class CreateOrder : OrderAction { }

            public class OrderHandler : IMessageHandler<CreateOrder>
            {
                public IPolicy {|#0:Authorize|}(OrderAction action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.OrphanedAuthorizeMethodId)
            .WithLocation(0)
            .WithArguments("OrderHandler", "Authorize", "OrderAction", "IHandlerAuthorization");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Handler_AlreadyImplementsAuthorizationInterface_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public class DeleteOrder : IMessage { }

            public class CreateOrderHandler : IMessageHandler<CreateOrder>, IHandlerAuthorization<DeleteOrder>
            {
                public IPolicy Authorize(DeleteOrder action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        // PIPMED003's territory (a type-parameter mismatch on an interface that IS implemented) - not this rule's.
        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task AuthorizeMethod_IsCalledFromElsewhereInsideType_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public class CreateOrderHandler : IMessageHandler<CreateOrder>
            {
                public IPolicy Authorize(CreateOrder action) => IdentityPolicy.Authenticated();

                public async Task Handle(CreateOrder action, CancellationToken cancellationToken)
                {
                    var policy = Authorize(action);
                    await Task.CompletedTask;
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task AuthorizeMethod_IsCalledFromAnotherPartOfPartialType_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public partial class CreateOrderHandler : IMessageHandler<CreateOrder>
            {
                public IPolicy Authorize(CreateOrder action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }

            public partial class CreateOrderHandler
            {
                public IPolicy CheckAccess(CreateOrder action) => Authorize(action);
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task AuthorizeMethod_ParameterIsUnrelatedType_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public class SomethingUnrelated { }

            public class CreateOrderHandler : IMessageHandler<CreateOrder>
            {
                public IPolicy Authorize(SomethingUnrelated action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task AuthorizeMethod_ReturnTypeIsNotExactlyIPolicy_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public class CustomPolicy : IPolicy
            {
                public Task<RuleSet> Resolve(System.IServiceProvider services, CancellationToken cancellationToken)
                    => Task.FromResult(new RuleSet(Operator.And));
            }

            public class CreateOrderHandler : IMessageHandler<CreateOrder>
            {
                public CustomPolicy Authorize(CreateOrder action) => new();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task AuthorizeAsyncMethod_MissingCancellationTokenParameter_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public class CreateOrderHandler : IMessageHandler<CreateOrder>
            {
                public Task<IPolicy> AuthorizeAsync(CreateOrder action)
                    => Task.FromResult<IPolicy>(IdentityPolicy.Authenticated());

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Method_NameDoesNotMatchExactly_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public class CreateOrderHandler : IMessageHandler<CreateOrder>
            {
                public IPolicy CanAuthorize(CreateOrder action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Method_IsNotPublic_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public class CreateOrderHandler : IMessageHandler<CreateOrder>
            {
                private IPolicy Authorize(CreateOrder action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Type_IsNotAHandler_DoesNotReport()
    {
        var source = """
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder { }

            public class Unrelated
            {
                public IPolicy Authorize(CreateOrder action) => IdentityPolicy.Authenticated();
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
