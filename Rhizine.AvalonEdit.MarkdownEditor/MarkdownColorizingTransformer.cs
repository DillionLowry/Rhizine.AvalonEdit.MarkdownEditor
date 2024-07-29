using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace Rhizine.AvalonEdit.MarkdownEditor;

public class MarkdownColorizingTransformer : DocumentColorizingTransformer
{
    public bool HideMarkdown { get; set; } = true;

    private static readonly Regex HeaderRegex = new Regex(@"^(#{1,6})\s", RegexOptions.Compiled);
    private static readonly Regex BoldItalicRegex = new Regex(@"(\*{1,3}|_{1,3})(?=\S)(.+?[*_]*)(?<=\S)\1", RegexOptions.Compiled);
    private static readonly Regex InlineCodeRegex = new Regex(@"(`+)(.+?)\1", RegexOptions.Compiled);
    private static readonly Regex LinkRegex = new Regex(@"\[([^\]]+)\]\(([^\)]+)\)", RegexOptions.Compiled);
    private static readonly Regex ImageRegex = new Regex(@"!\[([^\]]*)\]\(([^\)]+)\)", RegexOptions.Compiled);
    private static readonly Regex BlockquoteRegex = new Regex(@"^>\s", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex ListRegex = new Regex(@"^(\s*)([-*+]|\d+\.)\s", RegexOptions.Compiled | RegexOptions.Multiline);

    protected override void ColorizeLine(DocumentLine line)
    {
        if (!HideMarkdown) return;

        string lineText = CurrentContext.Document.GetText(line);

        // Headers
        var headerMatch = HeaderRegex.Match(lineText);
        if (headerMatch.Success)
        {
            HideText(line.Offset, headerMatch.Groups[1].Length + 1);
        }

        // Bold and Italic
        foreach (Match match in BoldItalicRegex.Matches(lineText))
        {
            HideText(line.Offset + match.Groups[1].Index, match.Groups[1].Length);
            HideText(line.Offset + match.Index + match.Length - match.Groups[1].Length, match.Groups[1].Length);
        }

        // Inline Code
        foreach (Match match in InlineCodeRegex.Matches(lineText))
        {
            HideText(line.Offset + match.Groups[1].Index, match.Groups[1].Length);
            HideText(line.Offset + match.Index + match.Length - match.Groups[1].Length, match.Groups[1].Length);
        }

        // Links
        foreach (Match match in LinkRegex.Matches(lineText))
        {
            HideText(line.Offset + match.Index, 1); // Opening [
            HideText(line.Offset + match.Groups[1].Index + match.Groups[1].Length, 2); // ](
            HideText(line.Offset + match.Index + match.Length - 1, 1); // Closing )
        }

        // Images
        foreach (Match match in ImageRegex.Matches(lineText))
        {
            HideText(line.Offset + match.Index, 2); // ![
            HideText(line.Offset + match.Groups[1].Index + match.Groups[1].Length, 2); // ](
            HideText(line.Offset + match.Index + match.Length - 1, 1); // Closing )
        }

        // Blockquotes
        var blockquoteMatch = BlockquoteRegex.Match(lineText);
        if (blockquoteMatch.Success)
        {
            HideText(line.Offset, blockquoteMatch.Length);
        }

        // Lists
        var listMatch = ListRegex.Match(lineText);
        if (listMatch.Success)
        {
            HideText(line.Offset + listMatch.Groups[1].Length, listMatch.Groups[2].Length + 1);
        }
    }

    private void HideText(int startOffset, int length)
    {
        ChangeLinePart(startOffset, startOffset + length, element =>
            element.TextRunProperties.SetForegroundBrush(Brushes.Transparent));
    }
}