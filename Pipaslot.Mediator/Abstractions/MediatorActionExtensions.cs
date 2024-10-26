using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Pipaslot.Mediator.Abstractions;

public static class MediatorActionExtensions
{
    internal static string GetActionName(this IMediatorAction? action)
    {
        return action?.GetType()?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Returns User friendly formated name. For examply class type "MyApp.GetUserListAction" will be formated as "Get user list"
    /// </summary>
    public static string GetActionFriendlyName(this IMediatorAction action, int ignoreLastWords = 1)
    {
        var name = action.GetActionName();
        return GetActionFriendlyName(name, ignoreLastWords);
    }

    public static string GetActionFriendlyName(this string actionName, int ignoreLastWords = 1)
    {
        if (string.IsNullOrWhiteSpace(actionName))
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
        var relevant = split.Take(split.Count() - ignoreLastWords);
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