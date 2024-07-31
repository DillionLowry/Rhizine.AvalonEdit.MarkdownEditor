using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace Rhizine.AvalonEdit.MarkdownEditor;

public class MarkdownColorizingTransformer : DocumentColorizingTransformer
{
    private static readonly Regex HeaderRegex = new Regex(@"^(#{1,6})\s", RegexOptions.Compiled);
    private static readonly Regex BoldItalicRegex = new Regex(@"(\*{1,3}|_{1,3})(?=\S)(.+?[*_]*)(?<=\S)\1", RegexOptions.Compiled);
    private static readonly Regex InlineCodeRegex = new Regex(@"(`+)(.+?)\1", RegexOptions.Compiled);
    private static readonly Regex LinkRegex = new Regex(@"\[([^\]]+)\]\(([^\)]+)\)", RegexOptions.Compiled);
    private static readonly Regex ImageRegex = new Regex(@"!\[([^\]]*)\]\(([^\)]+)\)", RegexOptions.Compiled);
    private static readonly Regex BlockquoteRegex = new Regex(@"^>\s", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex UnorderedListRegex = new Regex(@"^(\s*)[-*+]\s", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex OrderedListRegex = new Regex(@"^(\s*)\d+\.\s", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex TableRegex = new Regex(@"^\|(.+)\|$", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex HorizontalRuleRegex = new Regex(@"^(\*{3,}|-{3,}|_{3,})$", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex StrikethroughRegex = new Regex(@"~~(?=\S)(.+?)(?<=\S)~~", RegexOptions.Compiled);
    private static readonly Regex CodeBlockRegex = new Regex(@"^```[\s\S]*?^```", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex TaskListRegex = new Regex(@"^(\s*[-*+]\s)(\[[ x]\])\s", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex FootnoteRegex = new Regex(@"^\[\^(.+?)\]:", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex DefinitionListRegex = new Regex(@"^(.+)\n:\s", RegexOptions.Compiled | RegexOptions.Multiline);
    private static readonly Regex CodeBlockStartRegex = new Regex(@"^```\s?(\w+)?$", RegexOptions.Compiled);
    private static readonly Regex CodeBlockEndRegex = new Regex(@"^```$", RegexOptions.Compiled);


    public bool HideMarkdown { get; set; } = true;
    public bool IsEditMode { get; set; } = true;


    protected override void ColorizeLine(DocumentLine line)
    {
        if (!HideMarkdown && !IsEditMode) return;

        bool isInCodeBlock = false;
        int codeBlockStartLine = -1;

        string lineText = CurrentContext.Document.GetText(line);
        int lineStartOffset = line.Offset;

        var codeBlockStartMatch = CodeBlockStartRegex.Match(lineText);
        if (codeBlockStartMatch.Success)
        {
            isInCodeBlock = true;
            codeBlockStartLine = line.LineNumber;
            if (HideMarkdown)
            {
                HideText(lineStartOffset, codeBlockStartMatch.Length);
            }
            return;
        }

        // Check for code block end
        var codeBlockEndMatch = CodeBlockEndRegex.Match(lineText);
        if (codeBlockEndMatch.Success && isInCodeBlock)
        {
            isInCodeBlock = false;
            if (HideMarkdown)
            {
                HideText(lineStartOffset, line.Length);
            }
            return;
        }

        // If we're in a code block, highlight or collapse the entire line
        if (isInCodeBlock)
        {
            if (HideMarkdown)
            {
                HideText(lineStartOffset, line.Length);
            }
            else if (IsEditMode)
            {
                HighlightText(lineStartOffset, line.Length, Brushes.LightGray);
            }
            return;
        }

        // Headers
        var headerMatch = HeaderRegex.Match(lineText);
        if (headerMatch.Success)
        {
            if (HideMarkdown)
            {
                CollapseText(lineStartOffset, headerMatch.Groups[1].Length + 1);
            }
            if (IsEditMode)
            {
                HighlightText(lineStartOffset, line.Length, GetHeaderBrush(headerMatch.Groups[1].Length));
            }
        }

        // Code Blocks
        if (IsWithinCodeBlock(line) && IsEditMode)
        {
            HighlightText(lineStartOffset, line.Length, Brushes.LightGray);
        }

        // Blockquotes
        var blockquoteMatch = BlockquoteRegex.Match(lineText);
        if (blockquoteMatch.Success)
        {
            if (HideMarkdown)
            {
                CollapseText(lineStartOffset, blockquoteMatch.Length);
            }
            if (IsEditMode)
            {
                HighlightText(lineStartOffset, line.Length, Brushes.LightBlue);
            }
        }

        if (HideMarkdown)
        {
            // Bold and Italic
            foreach (Match match in BoldItalicRegex.Matches(lineText))
            {
                CollapseText(lineStartOffset + match.Groups[1].Index, match.Groups[1].Length);
                CollapseText(lineStartOffset + match.Index + match.Length - match.Groups[1].Length, match.Groups[1].Length);
            }

            // Inline Code
            foreach (Match match in InlineCodeRegex.Matches(lineText))
            {
                CollapseText(lineStartOffset + match.Groups[1].Index, match.Groups[1].Length);
                CollapseText(lineStartOffset + match.Index + match.Length - match.Groups[1].Length, match.Groups[1].Length);
            }

            // Links
            foreach (Match match in LinkRegex.Matches(lineText))
            {
                CollapseText(lineStartOffset + match.Index, 1); // Opening [
                CollapseText(lineStartOffset + match.Groups[1].Index + match.Groups[1].Length, 2); // ]( 
                CollapseText(lineStartOffset + match.Index + match.Length - 1, 1); // Closing )
            }

            // Images
            foreach (Match match in ImageRegex.Matches(lineText))
            {
                CollapseText(lineStartOffset + match.Index, 2); // ![
                CollapseText(lineStartOffset + match.Groups[1].Index + match.Groups[1].Length, 2); // ](
                CollapseText(lineStartOffset + match.Index + match.Length - 1, 1); // Closing )
            }

            // Unordered Lists
            var unorderedListMatch = UnorderedListRegex.Match(lineText);
            if (unorderedListMatch.Success)
            {
                CollapseText(lineStartOffset + unorderedListMatch.Groups[1].Length, 2);
            }

            // Ordered Lists
            var orderedListMatch = OrderedListRegex.Match(lineText);
            if (orderedListMatch.Success)
            {
                int numberLength = lineText.Substring(orderedListMatch.Groups[1].Length).TakeWhile(char.IsDigit).Count();
                CollapseText(lineStartOffset + orderedListMatch.Groups[1].Length, numberLength + 2);
            }

            // Tables
            var tableMatch = TableRegex.Match(lineText);
            if (tableMatch.Success)
            {
                CollapseText(lineStartOffset, 1); // Opening |
                CollapseText(lineStartOffset + lineText.Length - 1, 1); // Closing |
                var cells = tableMatch.Groups[1].Value.Split('|');
                int offset = 1;
                foreach (var cell in cells)
                {
                    if (cell.Trim().All(c => c == '-')) // Header separator
                    {
                        CollapseText(lineStartOffset + offset, cell.Length);
                    }
                    offset += cell.Length + 1;
                }
            }

            // Horizontal Rules
            var horizontalRuleMatch = HorizontalRuleRegex.Match(lineText);
            if (horizontalRuleMatch.Success)
            {
                CollapseText(lineStartOffset, horizontalRuleMatch.Length);
            }

            // Strikethrough
            foreach (Match match in StrikethroughRegex.Matches(lineText))
            {
                CollapseText(lineStartOffset + match.Index, 2); // Opening ~~
                CollapseText(lineStartOffset + match.Index + match.Length - 2, 2); // Closing ~~
            }

            // Task Lists
            var taskListMatch = TaskListRegex.Match(lineText);
            if (taskListMatch.Success)
            {
                CollapseText(lineStartOffset + taskListMatch.Groups[1].Length, taskListMatch.Groups[2].Length + 1);
            }

            // Footnotes
            var footnoteMatch = FootnoteRegex.Match(lineText);
            if (footnoteMatch.Success)
            {
                CollapseText(lineStartOffset, footnoteMatch.Length);
            }

            // Definition Lists
            var definitionListMatch = DefinitionListRegex.Match(lineText);
            if (definitionListMatch.Success)
            {
                CollapseText(lineStartOffset + definitionListMatch.Groups[1].Length, 2);
            }
        }
        bool IsWithinCodeBlock(DocumentLine line) // inline method
        {
            return isInCodeBlock && line.LineNumber > codeBlockStartLine;
        }
    }

    private void HideText(int startOffset, int length)
    {
        ChangeLinePart(startOffset, startOffset + length, element =>
        {
            element.TextRunProperties.SetForegroundBrush(Brushes.Transparent);
            element.TextRunProperties.SetBackgroundBrush(Brushes.Transparent);
        });
    }

    private void HighlightText(int startOffset, int length, Brush brush)
    {
        ChangeLinePart(startOffset, startOffset + length, element =>
        {
            element.TextRunProperties.SetBackgroundBrush(brush);
        });
    }
    private void CollapseText(int startOffset, int length)
    {
        ChangeLinePart(startOffset, startOffset + length, element =>
        {
            element.TextRunProperties.SetFontRenderingEmSize(0.01);
            element.TextRunProperties.SetForegroundBrush(Brushes.Transparent);
            element.TextRunProperties.SetBackgroundBrush(Brushes.Transparent);
        });
    }

    private Brush GetHeaderBrush(int level)
    {
        switch (level)
        {
            case 1: return Brushes.LightPink;
            case 2: return Brushes.LightGreen;
            case 3: return Brushes.LightYellow;
            case 4: return Brushes.LightCyan;
            case 5: return Brushes.LightGoldenrodYellow;
            case 6: return Brushes.LightSalmon;
            default: return Brushes.Transparent;
        }
    }

}

