using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pipaslot.Mediator.Abstractions;

/// <summary>
/// Turns an action type name into a human-readable label, e.g. for a spinner text, a log line or an error message shown to a user.
/// </summary>
public static class MediatorActionExtensions
{
    internal static string GetActionName(this IMediatorAction? action)
    {
        return action?.GetType().ToString() ?? string.Empty;
    }

    /// <summary>
    /// Returns User friendly formated name. For examply class type "MyApp.GetUserListAction" will be formated as "Get user list"
    /// </summary>
    public static string GetActionFriendlyName(this IMediatorAction action, int ignoreLastWords = 1)
    {
        var name = action.GetActionName();
        return GetActionFriendlyName(name, ignoreLastWords);
    }

    /// <summary>
    /// Formats a full type name the same way as <see cref="GetActionFriendlyName(IMediatorAction, int)"/>, for code that
    /// has only the name (e.g. received over the wire) and not the action instance.
    /// </summary>
    /// <remarks>
    /// The namespace is dropped, PascalCase words are split and lower-cased after the first one, and the nested-type separator
    /// <c>+</c> or an underscore also starts a new word. Returns an empty string for a null or whitespace name.
    /// </remarks>
    /// <param name="actionName">Full type name, e.g. <c>MyApp.GetUserListAction</c>.</param>
    /// <param name="ignoreLastWords">How many trailing words to drop - by default one, the <c>Action</c>/<c>Query</c>-style suffix.</param>
    public static string GetActionFriendlyName(this string? actionName, int ignoreLastWords = 1)
    {
        if (actionName is null || string.IsNullOrWhiteSpace(actionName))
        {
            return string.Empty;
        }

        var lastNamespaceDot = actionName.LastIndexOf('.');
        var startIndex = lastNamespaceDot < 0
            ? 0
            : lastNamespaceDot + 1;
        var className = actionName.Substring(startIndex);
        var cased = Regex.Replace(className, "([a-z0-9])([A-Z]*)([A-Z])", FormatUpercases);
        var noUnd = Regex.Replace(cased, "[_\\+]([A-Za-z0-9])", FormatUnderline);
        var split = noUnd.Split(' ');
        var relevant = split.Take(split.Length - ignoreLastWords);
        return string.Join(" ", relevant);
    }

    private static string FormatUnderline(Match match)
    {
        return $" {match.Groups[1].Value.ToLower()}";
    }

    private static string FormatUpercases(Match match)
    {
        var sb = new StringBuilder();
        sb.Append(match.Groups[1].Value);
        sb.Append(" ");
        if (match.Groups[2].Value.Length > 0)
        {
            sb.Append(match.Groups[2].Value);
            sb.Append(" ");
        }

        sb.Append(match.Groups[3].Value.ToLower());

        return sb.ToString();
    }
}