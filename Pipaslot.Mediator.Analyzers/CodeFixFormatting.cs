using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;

namespace Pipaslot.Mediator.Analyzers;

/// <summary>
/// Formatting helper shared by every code fix provider in this project that inserts new syntax nodes annotated
/// with <see cref="Formatter.Annotation"/>.
/// </summary>
internal static class CodeFixFormatting
{
    /// <summary>
    /// Runs <see cref="Formatter.Format(SyntaxNode, SyntaxAnnotation, Workspace, OptionSet, CancellationToken)"/>
    /// against <see cref="Formatter.Annotation"/>-marked nodes, forcing the <see cref="FormattingOptions.NewLine"/>
    /// option to match the original document's own line-ending convention - the formatter otherwise defaults new
    /// trivia to <see cref="System.Environment.NewLine"/>, which would leave the file with mixed CRLF/LF line
    /// endings on Windows whenever the original document used bare LF.
    /// </summary>
    public static async Task<Document> FormatMatchingDocumentNewLineAsync(Document document, SyntaxNode newRoot, CancellationToken cancellationToken)
    {
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var newLine = sourceText.ToString().Contains("\r\n") ? "\r\n" : "\n";
        var options = document.Project.Solution.Workspace.Options.WithChangedOption(FormattingOptions.NewLine, LanguageNames.CSharp, newLine);

        var formattedRoot = Formatter.Format(newRoot, Formatter.Annotation, document.Project.Solution.Workspace, options, cancellationToken);

        return document.WithSyntaxRoot(formattedRoot);
    }
}
