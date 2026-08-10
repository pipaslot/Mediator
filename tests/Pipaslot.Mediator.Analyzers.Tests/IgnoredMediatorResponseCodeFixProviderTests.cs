using System.Threading.Tasks;

namespace Pipaslot.Mediator.Analyzers.Tests;

using VerifyCS = CSharpCodeFixVerifier<IgnoredMediatorResponseAnalyzer, IgnoredMediatorResponseCodeFixProvider>;

/// <summary>
/// Covers <see cref="IgnoredMediatorResponseCodeFixProvider"/>'s three independent fixes for
/// <see cref="DiagnosticDescriptors.IgnoredMediatorResponseId"/>: capturing the result and guarding on
/// <c>.Failure</c> (index 0), rewriting the call to the corresponding <c>*Unhandled</c> method (index 1), and
/// rewriting the call to an explicit <c>_ = ...</c> discard (index 2).
/// </summary>
public class IgnoredMediatorResponseCodeFixProviderTests
{
    private const string Preamble = """
        using System.Threading;
        using System.Threading.Tasks;
        using Pipaslot.Mediator;

        public class SomeMessage : IMessage { }

        public class SomeRequest : IRequest<int> { }

        """;

    [Fact]
    public async Task Dispatch_SuccessCheckFix_CapturesResultAndGuardsOnFailure()
    {
        var source = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    await {|#0:mediator.Dispatch(new SomeMessage())|};
                }
            }
            """;
        var fixedSource = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    var result = await mediator.Dispatch(new SomeMessage());
                    if (result.Failure)
                    {
                    }
                }
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.IgnoredMediatorResponseId)
            .WithLocation(0)
            .WithArguments("Dispatch");

        await VerifyCS.VerifyCodeFixAsync(source, expected, fixedSource, codeActionIndex: 0);
    }

    [Fact]
    public async Task Dispatch_SuccessCheckFix_AvoidsShadowingExistingResultVariable()
    {
        var source = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    var result = 1;
                    await {|#0:mediator.Dispatch(new SomeMessage())|};
                }
            }
            """;
        var fixedSource = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    var result = 1;
                    var result1 = await mediator.Dispatch(new SomeMessage());
                    if (result1.Failure)
                    {
                    }
                }
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.IgnoredMediatorResponseId)
            .WithLocation(0)
            .WithArguments("Dispatch");

        await VerifyCS.VerifyCodeFixAsync(source, expected, fixedSource, codeActionIndex: 0);
    }

    [Fact]
    public async Task Execute_SuccessCheckFix_CapturesResultAndGuardsOnFailure()
    {
        var source = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    await {|#0:mediator.Execute(new SomeRequest())|};
                }
            }
            """;
        var fixedSource = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    var result = await mediator.Execute(new SomeRequest());
                    if (result.Failure)
                    {
                    }
                }
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.IgnoredMediatorResponseId)
            .WithLocation(0)
            .WithArguments("Execute");

        await VerifyCS.VerifyCodeFixAsync(source, expected, fixedSource, codeActionIndex: 0);
    }

    [Fact]
    public async Task Dispatch_UseUnhandledFix_RewritesCallToDispatchUnhandled()
    {
        var source = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    await {|#0:mediator.Dispatch(new SomeMessage())|};
                }
            }
            """;
        var fixedSource = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    await mediator.DispatchUnhandled(new SomeMessage());
                }
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.IgnoredMediatorResponseId)
            .WithLocation(0)
            .WithArguments("Dispatch");

        await VerifyCS.VerifyCodeFixAsync(source, expected, fixedSource, codeActionIndex: 1);
    }

    [Fact]
    public async Task Execute_UseUnhandledFix_RewritesCallToExecuteUnhandledPreservingExplicitTypeArgument()
    {
        var source = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    await {|#0:mediator.Execute<int>(new SomeRequest())|};
                }
            }
            """;
        var fixedSource = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    await mediator.ExecuteUnhandled<int>(new SomeRequest());
                }
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.IgnoredMediatorResponseId)
            .WithLocation(0)
            .WithArguments("Execute");

        await VerifyCS.VerifyCodeFixAsync(source, expected, fixedSource, codeActionIndex: 1);
    }

    [Fact]
    public async Task Dispatch_DiscardFix_RewritesCallToExplicitUnderscoreDiscard()
    {
        var source = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    await {|#0:mediator.Dispatch(new SomeMessage())|};
                }
            }
            """;
        var fixedSource = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    _ = await mediator.Dispatch(new SomeMessage());
                }
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.IgnoredMediatorResponseId)
            .WithLocation(0)
            .WithArguments("Dispatch");

        await VerifyCS.VerifyCodeFixAsync(source, expected, fixedSource, codeActionIndex: 2);
    }
}
