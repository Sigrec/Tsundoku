using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Tsundoku.Helpers;

/// <summary>
/// Self-contained Markdown-to-Inlines renderer for Avalonia's
/// <see cref="SelectableTextBlock"/>. Supports the common note-writing cases:
/// headings (# / ## / ###), **bold**, *italic*, ~~strikethrough~~, `inline code`,
/// fenced code blocks (```), [text](url), - / * bullet lists, 1. numbered lists,
/// > blockquotes, and --- / *** horizontal rules. Blank lines produce paragraph
/// gaps. Not a full CommonMark implementation; tables and nested lists render
/// as literal text.
/// </summary>
public static class MarkdownRenderer
{
    private static readonly FontFamily MonoFont = new("Consolas, Menlo, monospace");

    public static void ApplyTo(SelectableTextBlock target, string? markdown)
    {
        target.Inlines?.Clear();
        target.Inlines ??= new InlineCollection();

        if (string.IsNullOrWhiteSpace(markdown)) return;

        string[] lines = markdown.Replace("\r\n", "\n").Split('\n');
        bool inFencedBlock = false;
        StringBuilder fencedBuffer = new();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            if (line.StartsWith("```", StringComparison.Ordinal))
            {
                if (inFencedBlock)
                {
                    target.Inlines.Add(new Run(fencedBuffer.ToString().TrimEnd('\n'))
                    {
                        FontFamily = MonoFont,
                        FontSize = 12,
                    });
                    fencedBuffer.Clear();
                    inFencedBlock = false;
                }
                else
                {
                    inFencedBlock = true;
                }
                if (i < lines.Length - 1) target.Inlines.Add(new LineBreak());
                continue;
            }

            if (inFencedBlock)
            {
                fencedBuffer.Append(line);
                if (i < lines.Length - 1) fencedBuffer.Append('\n');
                continue;
            }

            if (IsHorizontalRule(line))
            {
                target.Inlines.Add(new Run(new string('─', 40))
                {
                    Foreground = new SolidColorBrush(Colors.Gray, 0.5),
                });
                if (i < lines.Length - 1) target.Inlines.Add(new LineBreak());
                continue;
            }

            if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                target.Inlines.Add(new Run(line[4..]) { FontWeight = FontWeight.Bold, FontSize = 15 });
            }
            else if (line.StartsWith("## ", StringComparison.Ordinal))
            {
                target.Inlines.Add(new Run(line[3..]) { FontWeight = FontWeight.Bold, FontSize = 17 });
            }
            else if (line.StartsWith("# ", StringComparison.Ordinal))
            {
                target.Inlines.Add(new Run(line[2..]) { FontWeight = FontWeight.Bold, FontSize = 20 });
            }
            else if (line.StartsWith("> ", StringComparison.Ordinal))
            {
                target.Inlines.Add(new Run("┃ ") { Foreground = new SolidColorBrush(Colors.Gray, 0.7) });
                Span quoteSpan = new() { FontStyle = FontStyle.Italic, Foreground = new SolidColorBrush(Colors.Gray, 0.85) };
                AppendInlines(quoteSpan.Inlines, line[2..]);
                target.Inlines.Add(quoteSpan);
            }
            else if (line.Length >= 2 && (line[0] == '-' || line[0] == '*') && line[1] == ' ')
            {
                target.Inlines.Add(new Run("•  ") { FontWeight = FontWeight.Bold });
                AppendInlines(target.Inlines, line[2..]);
            }
            else if (TryParseNumberedListMarker(line, out int prefixLen))
            {
                target.Inlines.Add(new Run(line[..prefixLen]) { FontWeight = FontWeight.Bold });
                AppendInlines(target.Inlines, line[prefixLen..]);
            }
            else
            {
                AppendInlines(target.Inlines, line);
            }

            if (i < lines.Length - 1)
            {
                target.Inlines.Add(new LineBreak());
            }
        }

        // Emit any unclosed fenced block as literal so users see their text back.
        if (inFencedBlock && fencedBuffer.Length > 0)
        {
            target.Inlines.Add(new Run(fencedBuffer.ToString()) { FontFamily = MonoFont, FontSize = 12 });
        }
    }

    private static bool IsHorizontalRule(string line)
    {
        string trimmed = line.Trim();
        if (trimmed.Length < 3) return false;
        char c = trimmed[0];
        if (c != '-' && c != '*' && c != '_') return false;
        foreach (char ch in trimmed) if (ch != c) return false;
        return true;
    }

    private static bool TryParseNumberedListMarker(string line, out int prefixLength)
    {
        prefixLength = 0;
        int i = 0;
        while (i < line.Length && char.IsDigit(line[i])) i++;
        if (i == 0 || i >= line.Length - 1) return false;
        if (line[i] != '.' || line[i + 1] != ' ') return false;
        prefixLength = i + 2;
        return true;
    }

    private static void AppendInlines(InlineCollection inlines, string text)
    {
        int i = 0;
        StringBuilder plain = new();

        while (i < text.Length)
        {
            char c = text[i];

            if (c == '*' && i + 1 < text.Length && text[i + 1] == '*')
            {
                int end = text.IndexOf("**", i + 2, StringComparison.Ordinal);
                if (end > i + 2)
                {
                    FlushPlain(inlines, plain);
                    inlines.Add(new Run(text[(i + 2)..end]) { FontWeight = FontWeight.Bold });
                    i = end + 2;
                    continue;
                }
            }

            if (c == '~' && i + 1 < text.Length && text[i + 1] == '~')
            {
                int end = text.IndexOf("~~", i + 2, StringComparison.Ordinal);
                if (end > i + 2)
                {
                    FlushPlain(inlines, plain);
                    inlines.Add(new Run(text[(i + 2)..end]) { TextDecorations = TextDecorations.Strikethrough });
                    i = end + 2;
                    continue;
                }
            }

            if (c == '*')
            {
                int end = text.IndexOf('*', i + 1);
                if (end > i + 1)
                {
                    FlushPlain(inlines, plain);
                    inlines.Add(new Run(text[(i + 1)..end]) { FontStyle = FontStyle.Italic });
                    i = end + 1;
                    continue;
                }
            }

            if (c == '_' && i + 1 < text.Length && text[i + 1] != '_')
            {
                int end = text.IndexOf('_', i + 1);
                if (end > i + 1)
                {
                    FlushPlain(inlines, plain);
                    inlines.Add(new Run(text[(i + 1)..end]) { FontStyle = FontStyle.Italic });
                    i = end + 1;
                    continue;
                }
            }

            if (c == '`')
            {
                int end = text.IndexOf('`', i + 1);
                if (end > i + 1)
                {
                    FlushPlain(inlines, plain);
                    inlines.Add(new Run(text[(i + 1)..end])
                    {
                        FontFamily = MonoFont,
                        FontSize = 12,
                    });
                    i = end + 1;
                    continue;
                }
            }

            if (c == '[')
            {
                int closeBracket = text.IndexOf(']', i + 1);
                if (closeBracket > i + 1 && closeBracket + 1 < text.Length && text[closeBracket + 1] == '(')
                {
                    int closeParen = text.IndexOf(')', closeBracket + 2);
                    if (closeParen > closeBracket + 2)
                    {
                        FlushPlain(inlines, plain);
                        string linkText = text[(i + 1)..closeBracket];
                        string url = text[(closeBracket + 2)..closeParen];
                        inlines.Add(new Run(linkText) { TextDecorations = TextDecorations.Underline });
                        inlines.Add(new Run($" ({url})") { FontSize = 11, Foreground = Brushes.Gray });
                        i = closeParen + 1;
                        continue;
                    }
                }
            }

            plain.Append(c);
            i++;
        }

        FlushPlain(inlines, plain);
    }

    private static void FlushPlain(InlineCollection inlines, StringBuilder sb)
    {
        if (sb.Length == 0) return;
        inlines.Add(new Run(sb.ToString()));
        sb.Clear();
    }
}
