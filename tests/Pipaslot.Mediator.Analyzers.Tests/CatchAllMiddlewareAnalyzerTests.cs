using System.Threading.Tasks;

namespace Pipaslot.Mediator.Analyzers.Tests;

using VerifyCS = CSharpAnalyzerVerifier<CatchAllMiddlewareAnalyzer>;

/// <summary>
/// Covers <see cref="CatchAllMiddlewareAnalyzer"/>: it must report a broad
/// <c>catch (Exception)</c> (or bare <c>catch</c>) only inside a class implementing
/// <c>IMediatorMiddleware</c>, and only when the guarded <c>try</c> block calls the <c>next</c>
/// continuation. It must not report on rethrows, calls to <c>context.AddException</c>, narrower
/// exception types, classes that are not middlewares, or a broad catch guarding unrelated code that
/// never calls <c>next</c>.
/// </summary>
public class CatchAllMiddlewareAnalyzerTests
{
    [Fact]
    public async Task Invoke_CatchesExceptionWithoutRethrow_ReportsDiagnostic()
    {
        var source = """
            using System;
            using System.Threading.Tasks;
            using Pipaslot.Mediator.Middlewares;

            public class SampleMiddleware : IMediatorMiddleware
            {
                public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
                {
                    try
                    {
                        await next(context);
                    }
                    {|#0:catch|} (Exception e)
                    {
                        context.AddError(e.Message);
                    }
                }
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.CatchAllMiddlewareId)
            .WithLocation(0)
            .WithArguments("SampleMiddleware");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Invoke_BareCatchWithoutRethrow_ReportsDiagnostic()
    {
        var source = """
            using System;
            using System.Threading.Tasks;
            using Pipaslot.Mediator.Middlewares;

            public class SampleMiddleware : IMediatorMiddleware
            {
                public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
                {
                    try
                    {
                        await next(context);
                    }
                    {|#0:catch|}
                    {
                        context.AddError("Something failed.");
                    }
                }
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.CatchAllMiddlewareId)
            .WithLocation(0)
            .WithArguments("SampleMiddleware");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Invoke_CatchesExceptionAndRethrows_DoesNotReport()
    {
        var source = """
            using System;
            using System.Threading.Tasks;
            using Pipaslot.Mediator.Middlewares;

            public class SampleMiddleware : IMediatorMiddleware
            {
                public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
                {
                    try
                    {
                        await next(context);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Invoke_CatchesExceptionAndRecordsViaAddException_DoesNotReport()
    {
        var source = """
            using System;
            using System.Threading.Tasks;
            using Pipaslot.Mediator.Middlewares;

            public class SampleMiddleware : IMediatorMiddleware
            {
                public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
                {
                    try
                    {
                        await next(context);
                    }
                    catch (Exception e)
                    {
                        context.AddException(e);
                    }
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Invoke_CatchesExceptionAndAddsErrorMessageAndAddException_DoesNotReport()
    {
        var source = """
            using System;
            using System.Threading.Tasks;
            using Pipaslot.Mediator.Middlewares;

            public class SampleMiddleware : IMediatorMiddleware
            {
                public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
                {
                    try
                    {
                        await next(context);
                    }
                    catch (Exception e)
                    {
                        context.AddError("The operation could not be completed.");
                        context.AddException(e);
                    }
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Invoke_CatchesNarrowerExceptionType_DoesNotReport()
    {
        var source = """
            using System;
            using System.Threading.Tasks;
            using Pipaslot.Mediator.Middlewares;

            public class SampleMiddleware : IMediatorMiddleware
            {
                public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
                {
                    try
                    {
                        await next(context);
                    }
                    catch (InvalidOperationException e)
                    {
                        context.AddError(e.Message);
                    }
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Invoke_CatchesExceptionAfterNextOutsideTry_DoesNotReport()
    {
        var source = """
            using System;
            using System.Threading.Tasks;
            using Pipaslot.Mediator.Middlewares;

            public class SampleMiddleware : IMediatorMiddleware
            {
                public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
                {
                    await next(context);

                    try
                    {
                        await Task.Delay(1000);
                    }
                    catch (Exception e)
                    {
                        context.AddError(e.Message);
                    }
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task Invoke_CatchesExceptionViaNextInvokeCall_ReportsDiagnostic()
    {
        var source = """
            using System;
            using System.Threading.Tasks;
            using Pipaslot.Mediator.Middlewares;

            public class SampleMiddleware : IMediatorMiddleware
            {
                public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
                {
                    try
                    {
                        await next.Invoke(context);
                    }
                    {|#0:catch|} (Exception e)
                    {
                        context.AddError(e.Message);
                    }
                }
            }
            """;
        var expected = VerifyCS.Diagnostic(DiagnosticDescriptors.CatchAllMiddlewareId)
            .WithLocation(0)
            .WithArguments("SampleMiddleware");

        await VerifyCS.VerifyAnalyzerAsync(source, expected);
    }

    [Fact]
    public async Task Invoke_CatchesExceptionOutsideMiddleware_DoesNotReport()
    {
        var source = """
            using System;

            public class PlainService
            {
                public void Run()
                {
                    try
                    {
                        DoWork();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                private void DoWork()
                {
                }
            }
            """;

        await VerifyCS.VerifyAnalyzerAsync(source);
    }
}
