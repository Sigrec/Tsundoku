using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace Tsundoku.Helpers;

/// <summary>
/// Self-contained Markdown-to-Inlines renderer for Avalonia's
/// <see cref="SelectableTextBlock"/>. Supports:
/// <list type="bullet">
/// <item>Headings <c># .. ######</c> (H1-H6)</item>
/// <item>Emphasis: <c>**bold**</c>, <c>*italic*</c> / <c>_italic_</c>, <c>~~strike~~</c>, <c>***bold+italic***</c></item>
/// <item>Inline code <c>`x`</c>, fenced code blocks <c>```..```</c></item>
/// <item>Explicit links <c>[text](url)</c>, reference links <c>[text][ref]</c> + <c>[ref]: url</c>, autolinks (bare http(s) URLs)</item>
/// <item>Bullet lists (- / *), numbered lists (1.)</item>
/// <item>Nested lists (2+ leading spaces = one indent level)</item>
/// <item>Blockquotes (&gt;), horizontal rules (---/***/___), footnotes (<c>[^1]</c> + <c>[^1]: text</c>)</item>
/// <item>Escapes: <c>\*</c>, <c>\`</c>, <c>\[</c>, <c>\_</c>, <c>\~</c>, <c>\\</c></item>
/// </list>
/// Not a full CommonMark implementation; tables, definition lists, and setext-style
/// headings render as literal text.
/// </summary>
public static class MarkdownRenderer
{
    private static readonly FontFamily MonoFont = new("Consolas, Menlo, monospace");
    private static readonly SolidColorBrush MutedBrush = new(Colors.Gray, 0.7);
    private static readonly SolidColorBrush QuoteBrush = new(Colors.Gray, 0.85);
    private static readonly string HorizontalRuleGlyph = new('─', 40);
    // Pre-built indentation prefixes for the common nesting depths (0..4 levels).
    // Deeper nests fall back to on-demand allocation.
    private static readonly string[] NestPrefixes = ["", "  ", "    ", "      ", "        "];

    public static void ApplyTo(
        SelectableTextBlock target,
        string? markdown,
        Action<string>? onLinkClicked = null)
    {
        target.Inlines?.Clear();
        target.Inlines ??= new InlineCollection();

        if (string.IsNullOrWhiteSpace(markdown)) return;

        string normalized = markdown.Contains('\r') ? markdown.Replace("\r\n", "\n") : markdown;
        string[] lines = normalized.Split('\n');

        // First pass: collect reference-link and footnote definitions and mark those
        // lines so the block pass skips them.
        Dictionary<string, string> linkRefs = new(StringComparer.OrdinalIgnoreCase);
        Dictionary<string, string> footnoteDefs = new(StringComparer.Ordinal);
        List<string> footnoteOrder = [];
        bool[] skip = new bool[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].TrimStart();
            if (TryParseFootnoteDef(trimmed, out string? fnKey, out string? fnText))
            {
                footnoteDefs[fnKey] = fnText;
                skip[i] = true;
                continue;
            }
            if (TryParseLinkRefDef(trimmed, out string? refKey, out string? refUrl))
            {
                linkRefs[refKey] = refUrl;
                skip[i] = true;
            }
        }

        InlineContext ctx = new(linkRefs, footnoteDefs, footnoteOrder, onLinkClicked);

        // Second pass: block-by-block emit.
        bool inFencedBlock = false;
        StringBuilder fencedBuffer = new();

        for (int i = 0; i < lines.Length; i++)
        {
            if (skip[i]) continue;
            string line = lines[i];

            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
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
                target.Inlines.Add(new Run(HorizontalRuleGlyph) { Foreground = MutedBrush });
                if (i < lines.Length - 1) target.Inlines.Add(new LineBreak());
                continue;
            }

            // Headings — H1..H6
            if (TryParseHeading(line, out int level, out string? headingText))
            {
                double size = level switch { 1 => 20, 2 => 17, 3 => 15, 4 => 14, 5 => 13, 6 => 12, _ => 13 };
                Span headingSpan = new() { FontWeight = FontWeight.Bold, FontSize = size };
                AppendInlines(headingSpan.Inlines, headingText, ctx);
                target.Inlines.Add(headingSpan);
                if (i < lines.Length - 1) target.Inlines.Add(new LineBreak());
                continue;
            }

            // Compute indent → nesting depth (2 spaces per level).
            int indent = CountLeadingSpaces(line);
            int nestDepth = indent / 2;
            string content = line[indent..];
            string nestPrefix = nestDepth <= 0
                ? string.Empty
                : nestDepth < NestPrefixes.Length ? NestPrefixes[nestDepth] : new string(' ', nestDepth * 2);

            if (content.Length >= 2 && (content[0] == '-' || content[0] == '*') && content[1] == ' ')
            {
                if (nestPrefix.Length > 0) target.Inlines.Add(new Run(nestPrefix));
                target.Inlines.Add(new Run(nestDepth > 0 ? "◦  " : "•  ") { FontWeight = FontWeight.Bold });
                AppendInlines(target.Inlines, content[2..], ctx);
            }
            else if (TryParseNumberedListMarker(content, out int numPrefixLen))
            {
                if (nestPrefix.Length > 0) target.Inlines.Add(new Run(nestPrefix));
                target.Inlines.Add(new Run(content[..numPrefixLen]) { FontWeight = FontWeight.Bold });
                AppendInlines(target.Inlines, content[numPrefixLen..], ctx);
            }
            else if (content.StartsWith("> ", StringComparison.Ordinal))
            {
                target.Inlines.Add(new Run("┃ ") { Foreground = MutedBrush });
                Span quoteSpan = new() { FontStyle = FontStyle.Italic, Foreground = QuoteBrush };
                AppendInlines(quoteSpan.Inlines, content[2..], ctx);
                target.Inlines.Add(quoteSpan);
            }
            else
            {
                AppendInlines(target.Inlines, line, ctx);
            }

            if (i < lines.Length - 1)
            {
                target.Inlines.Add(new LineBreak());
            }
        }

        if (inFencedBlock && fencedBuffer.Length > 0)
        {
            target.Inlines.Add(new Run(fencedBuffer.ToString()) { FontFamily = MonoFont, FontSize = 12 });
        }

        // Footnote footer — only if any footnotes were referenced during inline parsing.
        if (footnoteOrder.Count > 0)
        {
            target.Inlines.Add(new LineBreak());
            target.Inlines.Add(new LineBreak());
            target.Inlines.Add(new Run(HorizontalRuleGlyph) { Foreground = MutedBrush });
            target.Inlines.Add(new LineBreak());
            for (int idx = 0; idx < footnoteOrder.Count; idx++)
            {
                string key = footnoteOrder[idx];
                if (!footnoteDefs.TryGetValue(key, out string? text)) continue;
                target.Inlines.Add(new Run($"[{idx + 1}] ") { FontWeight = FontWeight.Bold, FontSize = 11 });
                Span note = new() { FontSize = 11, Foreground = MutedBrush };
                AppendInlines(note.Inlines, text, ctx);
                target.Inlines.Add(note);
                if (idx < footnoteOrder.Count - 1) target.Inlines.Add(new LineBreak());
            }
        }
    }

    private sealed class InlineContext
    {
        public Dictionary<string, string> LinkRefs { get; }
        public Dictionary<string, string> FootnoteDefs { get; }
        public List<string> FootnoteOrder { get; }
        public Action<string>? OnLinkClicked { get; }
        public InlineContext(Dictionary<string, string> linkRefs, Dictionary<string, string> footnoteDefs, List<string> footnoteOrder, Action<string>? onLinkClicked)
        {
            LinkRefs = linkRefs;
            FootnoteDefs = footnoteDefs;
            FootnoteOrder = footnoteOrder;
            OnLinkClicked = onLinkClicked;
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

    private static bool TryParseHeading(string line, out int level, out string text)
    {
        level = 0;
        text = string.Empty;
        int i = 0;
        while (i < line.Length && line[i] == '#' && i < 6) i++;
        if (i == 0 || i >= line.Length || line[i] != ' ') return false;
        level = i;
        text = line[(i + 1)..];
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

    private static bool TryParseLinkRefDef(string line, out string key, out string url)
    {
        key = string.Empty;
        url = string.Empty;
        if (line.Length < 5 || line[0] != '[') return false;
        int close = line.IndexOf(']', 1);
        if (close < 2 || close + 1 >= line.Length || line[close + 1] != ':') return false;
        string k = line[1..close].Trim();
        string u = line[(close + 2)..].Trim();
        if (k.Length == 0 || u.Length == 0 || k.StartsWith('^')) return false; // footnotes handled separately
        key = k;
        url = u;
        return true;
    }

    private static bool TryParseFootnoteDef(string line, out string key, out string text)
    {
        key = string.Empty;
        text = string.Empty;
        // [^key]: text
        if (line.Length < 6 || line[0] != '[' || line[1] != '^') return false;
        int close = line.IndexOf(']', 2);
        if (close < 3 || close + 1 >= line.Length || line[close + 1] != ':') return false;
        key = line[2..close];
        text = line[(close + 2)..].Trim();
        return key.Length > 0 && text.Length > 0;
    }

    private static int CountLeadingSpaces(string line)
    {
        int n = 0;
        while (n < line.Length && line[n] == ' ') n++;
        return n;
    }

    private static void AppendInlines(InlineCollection inlines, string text, InlineContext ctx)
    {
        int i = 0;
        StringBuilder plain = new();

        while (i < text.Length)
        {
            char c = text[i];

            // Escape: \c → literal c (skip parsing on the next char)
            if (c == '\\' && i + 1 < text.Length)
            {
                char next = text[i + 1];
                if (IsEscapable(next))
                {
                    plain.Append(next);
                    i += 2;
                    continue;
                }
            }

            // Bold+italic ***...***
            if (c == '*' && i + 2 < text.Length && text[i + 1] == '*' && text[i + 2] == '*')
            {
                int end = text.IndexOf("***", i + 3, StringComparison.Ordinal);
                if (end > i + 3)
                {
                    FlushPlain(inlines, plain);
                    inlines.Add(new Run(text[(i + 3)..end]) { FontWeight = FontWeight.Bold, FontStyle = FontStyle.Italic });
                    i = end + 3;
                    continue;
                }
            }

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

            // Footnote reference [^key]
            if (c == '[' && i + 1 < text.Length && text[i + 1] == '^')
            {
                int close = text.IndexOf(']', i + 2);
                if (close > i + 2)
                {
                    string key = text[(i + 2)..close];
                    if (ctx.FootnoteDefs.ContainsKey(key))
                    {
                        FlushPlain(inlines, plain);
                        int idx = ctx.FootnoteOrder.IndexOf(key);
                        if (idx < 0) { ctx.FootnoteOrder.Add(key); idx = ctx.FootnoteOrder.Count - 1; }
                        inlines.Add(new Run($"[{idx + 1}]")
                        {
                            FontSize = 10,
                            BaselineAlignment = BaselineAlignment.Top,
                            Foreground = MutedBrush,
                        });
                        i = close + 1;
                        continue;
                    }
                }
            }

            // Explicit link [text](url) or reference link [text][ref]
            if (c == '[')
            {
                int closeBracket = text.IndexOf(']', i + 1);
                if (closeBracket > i + 1 && closeBracket + 1 < text.Length)
                {
                    char afterClose = text[closeBracket + 1];
                    if (afterClose == '(')
                    {
                        int closeParen = text.IndexOf(')', closeBracket + 2);
                        if (closeParen > closeBracket + 2)
                        {
                            FlushPlain(inlines, plain);
                            string linkText = text[(i + 1)..closeBracket];
                            string url = text[(closeBracket + 2)..closeParen];
                            EmitLink(inlines, linkText, url, ctx);
                            i = closeParen + 1;
                            continue;
                        }
                    }
                    else if (afterClose == '[')
                    {
                        int closeRef = text.IndexOf(']', closeBracket + 2);
                        // >= (not >) so [text][] with an empty ref (shorthand for [text][text]) matches.
                        if (closeRef >= closeBracket + 2)
                        {
                            string linkText = text[(i + 1)..closeBracket];
                            string refKey = text[(closeBracket + 2)..closeRef];
                            if (string.IsNullOrEmpty(refKey)) refKey = linkText; // [text][] shorthand
                            if (ctx.LinkRefs.TryGetValue(refKey, out string? url))
                            {
                                FlushPlain(inlines, plain);
                                EmitLink(inlines, linkText, url, ctx);
                                i = closeRef + 1;
                                continue;
                            }
                        }
                    }
                }
            }

            // Autolink: bare http:// or https:// URL, terminated by whitespace or common punctuation.
            if ((c == 'h' || c == 'H') && LooksLikeUrl(text, i, out int urlEnd))
            {
                FlushPlain(inlines, plain);
                string url = text[i..urlEnd];
                EmitLink(inlines, url, url, ctx);
                i = urlEnd;
                continue;
            }

            plain.Append(c);
            i++;
        }

        FlushPlain(inlines, plain);
    }

    private static bool IsEscapable(char c) =>
        c is '\\' or '*' or '_' or '~' or '`' or '[' or ']' or '(' or ')' or '#' or '>' or '-' or '+' or '.' or '!';

    private static bool LooksLikeUrl(string text, int start, out int end)
    {
        end = start;
        ReadOnlySpan<char> span = text.AsSpan(start);
        if (span.Length < 7) return false;
        if (!(span.StartsWith("http://") || span.StartsWith("https://") || span.StartsWith("HTTP://") || span.StartsWith("HTTPS://"))) return false;

        int j = 0;
        while (j < span.Length)
        {
            char ch = span[j];
            if (char.IsWhiteSpace(ch) || ch == '<' || ch == '>' || ch == '"') break;
            j++;
        }
        // Trim trailing punctuation that's more likely sentence terminator than URL char.
        while (j > 0 && (span[j - 1] is '.' or ',' or ';' or ':' or ')' or ']' or '!' or '?')) j--;
        if (j < 8) return false; // "http://x" minimum-ish
        end = start + j;
        return true;
    }

    private static void EmitLink(InlineCollection inlines, string text, string url, InlineContext ctx)
    {
        TextBlock linkTb = new()
        {
            Text = text,
            TextDecorations = TextDecorations.Underline,
            FontWeight = FontWeight.Bold,
            Cursor = new Cursor(StandardCursorType.Hand),
            VerticalAlignment = VerticalAlignment.Center,
        };
        ToolTip.SetTip(linkTb, url);
        if (ctx.OnLinkClicked is not null)
        {
            Action<string> callback = ctx.OnLinkClicked;
            linkTb.PointerPressed += (_, _) => callback(url);
        }
        else
        {
            // No callback wired — still show the URL so the user can copy it manually.
            inlines.Add(new InlineUIContainer(linkTb) { BaselineAlignment = BaselineAlignment.Center });
            inlines.Add(new Run($" ({url})") { FontSize = 11, Foreground = MutedBrush });
            return;
        }
        inlines.Add(new InlineUIContainer(linkTb) { BaselineAlignment = BaselineAlignment.Center });
    }

    private static void FlushPlain(InlineCollection inlines, StringBuilder sb)
    {
        if (sb.Length == 0) return;
        // Explicit Normal so parent styling that inherits Bold (e.g. from a themed
        // window class) doesn't bleed into the plain runs.
        inlines.Add(new Run(sb.ToString()) { FontWeight = FontWeight.Normal });
        sb.Clear();
    }
}
