using System.Threading.Tasks;

namespace Pipaslot.Mediator.Analyzers.Tests;

using VerifyCS = CSharpAnalyzerVerifier<IgnoredMediatorResponseAnalyzer>;

/// <summary>
/// Covers <see cref="IgnoredMediatorResponseAnalyzer"/>: it must report a statement-level
/// <c>IMediator.Dispatch</c>/<c>IMediator.Execute&lt;TResult&gt;</c> call - awaited or not - whose
/// <c>IMediatorResponse</c> is discarded. It must not report <c>DispatchUnhandled</c>/<c>ExecuteUnhandled</c> (they
/// throw on failure instead), a call assigned to a variable or discarded via <c>_ = ...</c> (a different syntactic
/// shape than a bare <c>ExpressionStatement</c>), or a call passed as an argument to something else.
/// </summary>
public class IgnoredMediatorResponseAnalyzerTests
{
    private const string Preamble = """
        using System.Threading;
        using System.Threading.Tasks;
        using Pipaslot.Mediator;

        public class SomeMessage : IMessage { }

        public class SomeRequest : IRequest<int> { }

        """;

    [Fact]
    public async Task Dispatch_AwaitedAndDiscardedAsStatement_ReportsDiagnostic()
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
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.IgnoredMediatorResponseId)
            .WithLocation(0)
            .WithArguments("Dispatch");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Execute_AwaitedAndDiscardedAsStatement_ReportsDiagnostic()
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
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.IgnoredMediatorResponseId)
            .WithLocation(0)
            .WithArguments("Execute");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Dispatch_DiscardedWithoutAwait_ReportsDiagnostic()
    {
        var source = Preamble + """
            public class Caller
            {
                public void Run(IMediator mediator)
                {
                    {|#0:mediator.Dispatch(new SomeMessage())|};
                }
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.IgnoredMediatorResponseId)
            .WithLocation(0)
            .WithArguments("Dispatch");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Execute_DiscardedWithoutAwait_ReportsDiagnostic()
    {
        var source = Preamble + """
            public class Caller
            {
                public void Run(IMediator mediator)
                {
                    {|#0:mediator.Execute(new SomeRequest())|};
                }
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.IgnoredMediatorResponseId)
            .WithLocation(0)
            .WithArguments("Execute");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Dispatch_DiscardedExplicitlyViaUnderscore_DoesNotReport()
    {
        var source = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    _ = await mediator.Dispatch(new SomeMessage());
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Execute_DiscardedExplicitlyViaUnderscore_DoesNotReport()
    {
        var source = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    _ = await mediator.Execute(new SomeRequest());
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Dispatch_AssignedToVariable_DoesNotReport()
    {
        var source = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    var result = await mediator.Dispatch(new SomeMessage());
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Execute_AssignedToVariable_DoesNotReport()
    {
        var source = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    var result = await mediator.Execute(new SomeRequest());
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task DispatchUnhandled_DiscardedAsStatement_DoesNotReport()
    {
        var source = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    await mediator.DispatchUnhandled(new SomeMessage());
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ExecuteUnhandled_DiscardedAsStatement_DoesNotReport()
    {
        var source = Preamble + """
            public class Caller
            {
                public async Task Run(IMediator mediator)
                {
                    await mediator.ExecuteUnhandled(new SomeRequest());
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Dispatch_PassedAsArgumentToAnotherCall_DoesNotReport()
    {
        var source = Preamble + """
            public class Caller
            {
                public void Run(IMediator mediator)
                {
                    LogTask(mediator.Dispatch(new SomeMessage()));
                }

                private void LogTask(Task<IMediatorResponse> task) { }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Execute_PassedAsArgumentToAnotherCall_DoesNotReport()
    {
        var source = Preamble + """
            public class Caller
            {
                public void Run(IMediator mediator)
                {
                    LogTask(mediator.Execute(new SomeRequest()));
                }

                private void LogTask(Task<IMediatorResponse<int>> task) { }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
