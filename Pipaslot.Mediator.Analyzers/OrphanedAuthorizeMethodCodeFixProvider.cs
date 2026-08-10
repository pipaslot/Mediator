using System.Collections.Immutable;
using System.Composition;
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
/// Adds <c>IHandlerAuthorization&lt;TAction&gt;</c> (or <c>IHandlerAuthorizationAsync&lt;TAction&gt;</c> for an
/// <c>AuthorizeAsync</c> method) to the base list of the type reported by
/// <see cref="DiagnosticDescriptors.OrphanedAuthorizeMethodId"/>, using the flagged method's own parameter type as
/// <c>TAction</c> - the one unambiguous fix, since the method already has the right name, parameter, and return
/// type, and only the interface declaration is missing.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OrphanedAuthorizeMethodCodeFixProvider))]
[Shared]
public sealed class OrphanedAuthorizeMethodCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds { get; } =
        ImmutableArray.Create(DiagnosticDescriptors.OrphanedAuthorizeMethodId);

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var methodDeclaration = root.FindNode(diagnostic.Location.SourceSpan).FirstAncestorOrSelf<MethodDeclarationSyntax>();
        var typeDeclaration = methodDeclaration?.FirstAncestorOrSelf<TypeDeclarationSyntax>();
        if (methodDeclaration is null || typeDeclaration is null || methodDeclaration.ParameterList.Parameters.Count == 0)
        {
            return;
        }

        var actionType = methodDeclaration.ParameterList.Parameters[0].Type;
        if (actionType is null)
        {
            return;
        }

        var interfaceName = methodDeclaration.Identifier.ValueText == "AuthorizeAsync"
            ? "IHandlerAuthorizationAsync"
            : "IHandlerAuthorization";
        var title = $"Implement {interfaceName}<{actionType}>";

        context.RegisterCodeFix(
            CodeAction.Create(
                title,
                ct => AddAuthorizationInterfaceAsync(context.Document, typeDeclaration, interfaceName, actionType, ct),
                equivalenceKey: interfaceName),
            diagnostic);
    }

    private static async Task<Document> AddAuthorizationInterfaceAsync(
        Document document, TypeDeclarationSyntax typeDeclaration, string interfaceName, TypeSyntax actionType, CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null)
        {
            return document;
        }

        var interfaceType = SyntaxFactory.GenericName(SyntaxFactory.Identifier(interfaceName))
            .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList(actionType.WithoutTrivia())));
        var baseType = (BaseTypeSyntax)SyntaxFactory.SimpleBaseType(interfaceType);

        var newBaseList = typeDeclaration.BaseList is null
            ? SyntaxFactory.BaseList(SyntaxFactory.SingletonSeparatedList(baseType))
            : typeDeclaration.BaseList.AddTypes(baseType);

        var newTypeDeclaration = typeDeclaration.WithBaseList(newBaseList.WithAdditionalAnnotations(Formatter.Annotation));
        var newRoot = root.ReplaceNode(typeDeclaration, newTypeDeclaration);

        return await FormatMatchingDocumentNewLineAsync(document, newRoot, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Runs <see cref="Formatter.Format(SyntaxNode, SyntaxAnnotation, Workspace, OptionSet, CancellationToken)"/>
    /// against <see cref="Formatter.Annotation"/>-marked nodes, forcing the <see cref="FormattingOptions.NewLine"/>
    /// option to match the original document's own line-ending convention - the formatter otherwise defaults new
    /// trivia to <see cref="System.Environment.NewLine"/>, which would leave the file with mixed CRLF/LF line
    /// endings on Windows whenever the original document used bare LF.
    /// </summary>
    private static async Task<Document> FormatMatchingDocumentNewLineAsync(Document document, SyntaxNode newRoot, CancellationToken cancellationToken)
    {
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var newLine = sourceText.ToString().Contains("\r\n") ? "\r\n" : "\n";
        var options = document.Project.Solution.Workspace.Options.WithChangedOption(FormattingOptions.NewLine, LanguageNames.CSharp, newLine);

        var formattedRoot = Formatter.Format(newRoot, Formatter.Annotation, document.Project.Solution.Workspace, options, cancellationToken);

        return document.WithSyntaxRoot(formattedRoot);
    }
}
