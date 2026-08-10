using System.Threading.Tasks;

namespace Pipaslot.Mediator.Analyzers.Tests;

using VerifyCS = CSharpAnalyzerVerifier<MixedAuthorizationMechanismAnalyzer>;

/// <summary>
/// Covers <see cref="MixedAuthorizationMechanismAnalyzer"/>: it must report, for a handler and an action it
/// handles, when both sides have an active authorization source - regardless of mechanism kind, e.g. a policy
/// attribute on the action plus a different policy attribute on the handler - or when one class alone mixes an
/// interface (<c>IActionAuthorization</c>/<c>IHandlerAuthorization&lt;T&gt;</c>) with a policy attribute (e.g.
/// <c>AuthenticatedPolicyAttribute</c>). It must not report when every active source lives on a single class (one
/// attribute, several attributes, or the interface alone, with the other side declaring nothing), when at most one
/// source is active at all, or on a class that implements an authorization interface without implementing any
/// handler interface (out of scope for this rule).
/// </summary>
public class MixedAuthorizationMechanismAnalyzerTests
{
    [Fact]
    public async Task Handler_MixesInterfaceAndAttribute_ReportsDiagnostic()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            [AuthenticatedPolicy]
            public class {|#0:OrderHandler|} : IMessageHandler<CreateOrder>, IHandlerAuthorization<CreateOrder>
            {
                public IPolicy Authorize(CreateOrder action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.MixedAuthorizationMechanismId)
            .WithLocation(0)
            .WithArguments("OrderHandler", "CreateOrder",
                "an authorization interface on handler 'OrderHandler', [AuthenticatedPolicyAttribute] on handler 'OrderHandler'");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Action_MixesInterfaceAndAttribute_ReportsDiagnostic()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            [AnonymousPolicy]
            public class CreateOrder : IMessage, IActionAuthorization
            {
                public IPolicy Authorize() => IdentityPolicy.Anonymous();
            }

            public class {|#0:OrderHandler|} : IMessageHandler<CreateOrder>
            {
                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.MixedAuthorizationMechanismId)
            .WithLocation(0)
            .WithArguments("OrderHandler", "CreateOrder",
                "an authorization interface on action 'CreateOrder', [AnonymousPolicyAttribute] on action 'CreateOrder'");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task ActionUsesAttributeAndHandlerUsesInterface_ReportsDiagnostic()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            [AuthenticatedPolicy]
            public class CreateOrder : IMessage { }

            public class {|#0:OrderHandler|} : IMessageHandler<CreateOrder>, IHandlerAuthorization<CreateOrder>
            {
                public IPolicy Authorize(CreateOrder action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.MixedAuthorizationMechanismId)
            .WithLocation(0)
            .WithArguments("OrderHandler", "CreateOrder",
                "[AuthenticatedPolicyAttribute] on action 'CreateOrder', an authorization interface on handler 'OrderHandler'");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task ActionUsesInterfaceAndHandlerUsesAttribute_ReportsDiagnostic()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage, IActionAuthorization
            {
                public IPolicy Authorize() => IdentityPolicy.Anonymous();
            }

            [AuthenticatedPolicy]
            public class {|#0:OrderHandler|} : IMessageHandler<CreateOrder>
            {
                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.MixedAuthorizationMechanismId)
            .WithLocation(0)
            .WithArguments("OrderHandler", "CreateOrder",
                "an authorization interface on action 'CreateOrder', [AuthenticatedPolicyAttribute] on handler 'OrderHandler'");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task ActionAndHandlerBothUseAttributes_ReportsDiagnostic()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            [AuthenticatedPolicy]
            public class CreateOrder : IMessage { }

            [RolePolicy("Admin")]
            public class {|#0:OrderHandler|} : IMessageHandler<CreateOrder>
            {
                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.MixedAuthorizationMechanismId)
            .WithLocation(0)
            .WithArguments("OrderHandler", "CreateOrder",
                "[AuthenticatedPolicyAttribute] on action 'CreateOrder', [RolePolicyAttribute] on handler 'OrderHandler'");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task ActionAndHandlerBothUseInterfaces_ReportsDiagnostic()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage, IActionAuthorization
            {
                public IPolicy Authorize() => IdentityPolicy.Anonymous();
            }

            public class {|#0:OrderHandler|} : IMessageHandler<CreateOrder>, IHandlerAuthorization<CreateOrder>
            {
                public IPolicy Authorize(CreateOrder action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.MixedAuthorizationMechanismId)
            .WithLocation(0)
            .WithArguments("OrderHandler", "CreateOrder",
                "an authorization interface on action 'CreateOrder', an authorization interface on handler 'OrderHandler'");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task HandlerHasTwoAttributesOnSameClass_NoActionPolicy_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            [AuthenticatedPolicy]
            [RolePolicy("Admin")]
            public class OrderHandler : IMessageHandler<CreateOrder>
            {
                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ActionHasTwoAttributesOnSameClass_NoHandlerPolicy_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            [AuthenticatedPolicy]
            [RolePolicy("Admin")]
            public class CreateOrder : IMessage { }

            public class OrderHandler : IMessageHandler<CreateOrder>
            {
                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Handler_MixesAsyncInterfaceAndAttribute_ReportsDiagnostic()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            [AuthenticatedPolicy]
            public class {|#0:OrderHandler|} : IMessageHandler<CreateOrder>, IHandlerAuthorizationAsync<CreateOrder>
            {
                public Task<IPolicy> AuthorizeAsync(CreateOrder action, CancellationToken cancellationToken)
                    => Task.FromResult<IPolicy>(IdentityPolicy.Authenticated());

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.MixedAuthorizationMechanismId)
            .WithLocation(0)
            .WithArguments("OrderHandler", "CreateOrder",
                "an authorization interface on handler 'OrderHandler', [AuthenticatedPolicyAttribute] on handler 'OrderHandler'");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task OnlyHandlerAsyncInterface_NoActionPolicy_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public class OrderHandler : IMessageHandler<CreateOrder>, IHandlerAuthorizationAsync<CreateOrder>
            {
                public Task<IPolicy> AuthorizeAsync(CreateOrder action, CancellationToken cancellationToken)
                    => Task.FromResult<IPolicy>(IdentityPolicy.Authenticated());

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task OnlyHandlerInterface_NoActionPolicy_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public class OrderHandler : IMessageHandler<CreateOrder>, IHandlerAuthorization<CreateOrder>
            {
                public IPolicy Authorize(CreateOrder action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task OnlyActionAttribute_NoHandlerPolicy_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            [AnonymousPolicy]
            public class CreateOrder : IMessage { }

            public class OrderHandler : IMessageHandler<CreateOrder>
            {
                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task NoAuthorizationAtAll_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;

            public class CreateOrder : IMessage { }

            public class OrderHandler : IMessageHandler<CreateOrder>
            {
                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Type_ImplementsOnlyAuthorizationInterfaceWithoutHandlerInterface_DoesNotReport()
    {
        var source = """
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            [AuthenticatedPolicy]
            public class OrphanedAuthorization : IHandlerAuthorization<CreateOrder>
            {
                public IPolicy Authorize(CreateOrder action) => IdentityPolicy.Authenticated();
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
