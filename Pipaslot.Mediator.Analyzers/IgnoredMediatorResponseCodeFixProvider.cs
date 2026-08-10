using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;

namespace Pipaslot.Mediator.Analyzers;

/// <summary>
/// Offers three independent fixes for <see cref="DiagnosticDescriptors.IgnoredMediatorResponseId"/>: (a) capture
/// the discarded call into a local and guard the rest of the statement with <c>if (result.Failure) { }</c>, (b)
/// rewrite the call to the corresponding <c>*Unhandled</c> method, which throws on failure instead of reporting it
/// through a response that would otherwise go unread, or (c) rewrite the call to the idiomatic explicit discard
/// <c>_ = await mediator.Dispatch(x);</c>, for a caller that has already decided the response genuinely doesn't
/// matter here. All three are offered together because none is more "correct" than the others - it depends on
/// whether the caller wants to react to a failure, let it propagate, or is deliberately ignoring it.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IgnoredMediatorResponseCodeFixProvider))]
[Shared]
public sealed class IgnoredMediatorResponseCodeFixProvider : CodeFixProvider
{
    private const string SuccessCheckTitle = "Check the response before continuing";
    private const string DiscardTitle = "Discard the response explicitly (_ = ...)";
    private const string ResultVariableBaseName = "result";

    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.IgnoredMediatorResponseId);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var invocation = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (invocation is null || invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return;
        }

        var statement = invocation.FirstAncestorOrSelf<ExpressionStatementSyntax>();
        if (statement is not null)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    SuccessCheckTitle,
                    ct => AddSuccessCheckAsync(context.Document, statement, ct),
                    equivalenceKey: SuccessCheckTitle),
                diagnostic);
        }

        var unhandledMethodName = memberAccess.Name.Identifier.ValueText + "Unhandled";
        context.RegisterCodeFix(
            CodeAction.Create(
                $"Call {unhandledMethodName} instead",
                ct => RewriteToUnhandledAsync(context.Document, invocation, memberAccess, unhandledMethodName, ct),
                equivalenceKey: unhandledMethodName),
            diagnostic);

        if (statement is not null)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    DiscardTitle,
                    ct => DiscardResponseAsync(context.Document, statement, ct),
                    equivalenceKey: DiscardTitle),
                diagnostic);
        }
    }

    /// <summary>
    /// Replaces <c>[await ]mediator.Dispatch(x);</c> with <c>_ = [await ]mediator.Dispatch(x);</c> - the idiomatic
    /// way to tell both a reader and this analyzer that discarding the response is deliberate.
    /// </summary>
    private static async Task<Document> DiscardResponseAsync(Document document, ExpressionStatementSyntax statement, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var discardAssignment = SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                SyntaxFactory.IdentifierName(SyntaxFactory.Identifier("_")),
                statement.Expression)
            .WithAdditionalAnnotations(Formatter.Annotation);

        var newStatement = statement.WithExpression(discardAssignment);
        var newRoot = root.ReplaceNode(statement, newStatement);

        return await CodeFixFormatting.FormatMatchingDocumentNewLineAsync(document, newRoot, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Replaces <c>[await ]mediator.Dispatch(x);</c> with <c>var result = [await ]mediator.Dispatch(x); if
    /// (result.Failure) { }</c> - the original expression (await included, if present) becomes the local's
    /// initializer unchanged, so this never alters whether the call is awaited. <c>result.Failure</c> is used
    /// instead of <c>!result.Success</c> so the guard reads as what it handles (the failure branch) rather than a
    /// negated success check.
    /// </summary>
    private static async Task<Document> AddSuccessCheckAsync(Document document, ExpressionStatementSyntax statement, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        if (root is null || semanticModel is null)
        {
            return document;
        }

        var variableName = GetUniqueVariableName(semanticModel, statement.SpanStart, ResultVariableBaseName);

        var declaration = SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("var"))
                    .WithVariables(SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(SyntaxFactory.Identifier(variableName))
                            .WithInitializer(SyntaxFactory.EqualsValueClause(statement.Expression)))))
            .WithLeadingTrivia(statement.GetLeadingTrivia())
            .WithAdditionalAnnotations(Formatter.Annotation);

        var ifStatement = SyntaxFactory.IfStatement(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName(variableName),
                    SyntaxFactory.IdentifierName("Failure")),
                SyntaxFactory.Block())
            .WithTrailingTrivia(statement.GetTrailingTrivia())
            .WithAdditionalAnnotations(Formatter.Annotation);

        var newRoot = root.ReplaceNode(statement, new StatementSyntax[] { declaration, ifStatement });

        return await CodeFixFormatting.FormatMatchingDocumentNewLineAsync(document, newRoot, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Picks a local name that doesn't shadow anything already visible at the statement - appends an increasing
    /// numeric suffix to <paramref name="baseName"/> until <see cref="SemanticModel.LookupSymbols"/> finds nothing.
    /// </summary>
    private static string GetUniqueVariableName(SemanticModel semanticModel, int position, string baseName)
    {
        var name = baseName;
        var suffix = 1;
        while (semanticModel.LookupSymbols(position, name: name).Any())
        {
            name = baseName + suffix;
            suffix++;
        }

        return name;
    }

    /// <summary>
    /// Renames the invoked method in place - <c>Dispatch</c> to <c>DispatchUnhandled</c>, or <c>Execute&lt;T&gt;</c>
    /// to <c>ExecuteUnhandled&lt;T&gt;</c>, preserving any type argument list - without touching arguments, the
    /// receiver, or the surrounding await/statement shape.
    /// </summary>
    private static Task<Document> RewriteToUnhandledAsync(
        Document document,
        InvocationExpressionSyntax invocation,
        MemberAccessExpressionSyntax memberAccess,
        string unhandledMethodName,
        CancellationToken cancellationToken)
    {
        SimpleNameSyntax newName = memberAccess.Name is GenericNameSyntax generic
            ? generic.WithIdentifier(SyntaxFactory.Identifier(unhandledMethodName))
            : SyntaxFactory.IdentifierName(unhandledMethodName);

        var newInvocation = invocation.WithExpression(memberAccess.WithName(newName));

        return ReplaceInvocationAsync(document, invocation, newInvocation, cancellationToken);
    }

    private static async Task<Document> ReplaceInvocationAsync(
        Document document,
        InvocationExpressionSyntax oldInvocation,
        InvocationExpressionSyntax newInvocation,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var newRoot = root.ReplaceNode(oldInvocation, newInvocation);
        return document.WithSyntaxRoot(newRoot);
    }
}
