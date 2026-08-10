using System.Threading.Tasks;

namespace Pipaslot.Mediator.Analyzers.Tests;

using VerifyCS = CSharpAnalyzerVerifier<HandlerAuthorizationTypeMismatchAnalyzer>;

/// <summary>
/// Covers <see cref="HandlerAuthorizationTypeMismatchAnalyzer"/>: it must report, for each action a handler
/// handles, when none of the handler's <c>IHandlerAuthorization&lt;T&gt;</c>/<c>IHandlerAuthorizationAsync&lt;T&gt;</c>
/// type parameters is that action type or a base of it. It must not report when the authorized type is a base type
/// (or <c>IMediatorAction</c> itself) of every handled action, on an open generic handler whose type parameters
/// line up structurally, or on a class that implements no handler interface or no authorization interface at all.
/// </summary>
public class HandlerAuthorizationTypeMismatchAnalyzerTests
{
    [Fact]
    public async Task Handler_AuthorizesUnrelatedType_ReportsDiagnostic()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public class DeleteOrder : IMessage { }

            public class {|#0:CreateOrderHandler|} : IMessageHandler<CreateOrder>, IHandlerAuthorization<DeleteOrder>
            {
                public IPolicy Authorize(DeleteOrder action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.HandlerAuthorizationTypeMismatchId)
            .WithLocation(0)
            .WithArguments("CreateOrderHandler", "CreateOrder", "DeleteOrder");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Handler_HandlesTwoActionsAuthorizesOnlyOne_ReportsDiagnosticForUncoveredAction()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public class CancelOrder : IMessage { }

            public class {|#0:OrderHandler|} : IMessageHandler<CreateOrder>, IMessageHandler<CancelOrder>, IHandlerAuthorization<CreateOrder>
            {
                public IPolicy Authorize(CreateOrder action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;

                public Task Handle(CancelOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.HandlerAuthorizationTypeMismatchId)
            .WithLocation(0)
            .WithArguments("OrderHandler", "CancelOrder", "CreateOrder");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Handler_AuthorizesSharedBaseTypeOfHandledActions_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public abstract class OrderAction : IMessage { }

            public class CreateOrder : OrderAction { }

            public class CancelOrder : OrderAction { }

            public class OrderHandler : IMessageHandler<CreateOrder>, IMessageHandler<CancelOrder>, IHandlerAuthorization<OrderAction>
            {
                public IPolicy Authorize(OrderAction action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;

                public Task Handle(CancelOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Handler_AuthorizesIMediatorAction_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Abstractions;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public class CreateOrderHandler : IMessageHandler<CreateOrder>, IHandlerAuthorization<IMediatorAction>
            {
                public IPolicy Authorize(IMediatorAction action) => IdentityPolicy.Authenticated();

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Handler_IsOpenGeneric_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public interface IGenericMessage : IMessage { }

            public class GenericHandler<TAction> : IMessageHandler<TAction>, IHandlerAuthorization<TAction>
                where TAction : IGenericMessage
            {
                public IPolicy Authorize(TAction action) => IdentityPolicy.Authenticated();

                public Task Handle(TAction action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Handler_HasNoAuthorizationInterface_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;

            public class CreateOrder : IMessage { }

            public class CreateOrderHandler : IMessageHandler<CreateOrder>
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

            public class OrphanedAuthorization : IHandlerAuthorization<CreateOrder>
            {
                public IPolicy Authorize(CreateOrder action) => IdentityPolicy.Authenticated();
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Handler_AuthorizesMatchingTypeAsynchronously_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public class CreateOrderHandler : IMessageHandler<CreateOrder>, IHandlerAuthorizationAsync<CreateOrder>
            {
                public Task<IPolicy> AuthorizeAsync(CreateOrder action, CancellationToken cancellationToken)
                    => Task.FromResult<IPolicy>(IdentityPolicy.Authenticated());

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Handler_CombinesSyncAndAsyncAuthorizationForDifferentActions_DoesNotReport()
    {
        var source = """
            using System.Threading;
            using System.Threading.Tasks;
            using Pipaslot.Mediator;
            using Pipaslot.Mediator.Authorization;

            public class CreateOrder : IMessage { }

            public class CancelOrder : IMessage { }

            public class OrderHandler : IMessageHandler<CreateOrder>, IMessageHandler<CancelOrder>,
                IHandlerAuthorization<CreateOrder>, IHandlerAuthorizationAsync<CancelOrder>
            {
                public IPolicy Authorize(CreateOrder action) => IdentityPolicy.Authenticated();

                public Task<IPolicy> AuthorizeAsync(CancelOrder action, CancellationToken cancellationToken)
                    => Task.FromResult<IPolicy>(IdentityPolicy.Authenticated());

                public Task Handle(CreateOrder action, CancellationToken cancellationToken) => Task.CompletedTask;

                public Task Handle(CancelOrder action, CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
