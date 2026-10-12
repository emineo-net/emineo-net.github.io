using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace EmineoBlazor.Services;

/// <summary>
///     Minimal CommonMark-flavored renderer, tuned for the shape of our READMEs.
///     Not a full spec implementation — deliberately small and predictable.
/// </summary>
public static class SimpleMarkdown
{
    public static string ToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return "";
        }

        var lines = markdown.Replace("\r\n", "\n").Split('\n');
        var sb = new StringBuilder();
        var i = 0;

        while (i < lines.Length)
        {
            var line = lines[i];

            // -------- Code fence --------
            if (line.TrimStart().StartsWith("```"))
            {
                var lang = line.TrimStart().Substring(3).Trim();
                var code = new StringBuilder();
                i++;
                while (i < lines.Length && !lines[i].TrimStart().StartsWith("```"))
                {
                    code.AppendLine(lines[i]);
                    i++;
                }

                i++; // skip closing fence
                sb.Append(
                    $"<pre class=\"docs-code\"{(string.IsNullOrEmpty(lang) ? "" : $" data-lang=\"{WebUtility.HtmlEncode(lang)}\"")}><code>");
                sb.Append(WebUtility.HtmlEncode(code.ToString().TrimEnd('\n')));
                sb.Append("</code></pre>");
                continue;
            }

            // -------- Table --------
            if (line.Contains('|') && i + 1 < lines.Length && IsTableSeparator(lines[i + 1]))
            {
                sb.Append("<div class=\"table-responsive\"><table class=\"table docs-table\"><thead><tr>");
                foreach (var cell in SplitTableRow(line))
                {
                    sb.Append($"<th>{Inline(cell)}</th>");
                }

                sb.Append("</tr></thead><tbody>");
                i += 2;
                while (i < lines.Length && lines[i].Contains('|') && !string.IsNullOrWhiteSpace(lines[i]))
                {
                    sb.Append("<tr>");
                    foreach (var cell in SplitTableRow(lines[i]))
                    {
                        sb.Append($"<td>{Inline(cell)}</td>");
                    }

                    sb.Append("</tr>");
                    i++;
                }

                sb.Append("</tbody></table></div>");
                continue;
            }

            // -------- Headings --------
            var h = Regex.Match(line, @"^(#{1,6})\s+(.*)$");
            if (h.Success)
            {
                var level = h.Groups[1].Value.Length;
                var text = h.Groups[2].Value;
                var id = Slugify(text);
                sb.Append($"<h{level} id=\"{id}\">{Inline(text)}</h{level}>");
                i++;
                continue;
            }

            // -------- Horizontal rule --------
            if (Regex.IsMatch(line, @"^---+\s*$") || Regex.IsMatch(line, @"^\*\*\*+\s*$"))
            {
                sb.Append("<hr />");
                i++;
                continue;
            }

            // -------- Blockquote --------
            if (line.TrimStart().StartsWith("> ") || line.TrimStart() == ">")
            {
                var q = new StringBuilder();
                while (i < lines.Length && (lines[i].TrimStart().StartsWith("> ") || lines[i].TrimStart() == ">"))
                {
                    var stripped = lines[i].TrimStart();
                    stripped = stripped.Length >= 2 ? stripped.Substring(2) : "";
                    q.AppendLine(stripped);
                    i++;
                }

                sb.Append("<blockquote>");
                sb.Append(ToHtml(q.ToString()));
                sb.Append("</blockquote>");
                continue;
            }

            // -------- Unordered list --------
            if (Regex.IsMatch(line, @"^\s*[-*+]\s+"))
            {
                sb.Append("<ul>");
                while (i < lines.Length && Regex.IsMatch(lines[i], @"^\s*[-*+]\s+"))
                {
                    var itemText = Regex.Replace(lines[i], @"^\s*[-*+]\s+", "");
                    sb.Append($"<li>{Inline(itemText)}</li>");
                    i++;
                }

                sb.Append("</ul>");
                continue;
            }

            // -------- Ordered list --------
            if (Regex.IsMatch(line, @"^\s*\d+\.\s+"))
            {
                sb.Append("<ol>");
                while (i < lines.Length && Regex.IsMatch(lines[i], @"^\s*\d+\.\s+"))
                {
                    var itemText = Regex.Replace(lines[i], @"^\s*\d+\.\s+", "");
                    sb.Append($"<li>{Inline(itemText)}</li>");
                    i++;
                }

                sb.Append("</ol>");
                continue;
            }

            // -------- Blank line --------
            if (string.IsNullOrWhiteSpace(line))
            {
                i++;
                continue;
            }

            // -------- Paragraph (accumulate until blank or block start) --------
            var para = new StringBuilder();
            while (i < lines.Length && !string.IsNullOrWhiteSpace(lines[i]) && !lines[i].TrimStart().StartsWith("```") &&
                   !lines[i].TrimStart().StartsWith("#") && !lines[i].TrimStart().StartsWith("> ") &&
                   !Regex.IsMatch(lines[i], @"^\s*[-*+]\s+") && !Regex.IsMatch(lines[i], @"^\s*\d+\.\s+") &&
                   !Regex.IsMatch(lines[i], @"^---+\s*$"))
            {
                para.AppendLine(lines[i]);
                i++;
            }

            sb.Append($"<p>{Inline(para.ToString().Trim())}</p>");
        }

        return sb.ToString();
    }

    // ---------- inline formatting ----------
    private static string Inline(string text)
    {
        var result = text;

        // Inline code — escape the inside so < and > render literally.
        result = Regex.Replace(result, @"`([^`]+)`", m => $"<code class=\"docs-inline\">{WebUtility.HtmlEncode(m.Groups[1].Value)}</code>");

        // Bold **text**
        result = Regex.Replace(result, @"\*\*([^\*]+)\*\*", "<strong>$1</strong>");

        // Italic *text*
        result = Regex.Replace(result, @"(?<![\*\w])\*([^\*\n]+?)\*(?!\*)", "<em>$1</em>");

        // Links [text](url)
        result = Regex.Replace(result, @"\[([^\]]+)\]\(([^\)]+)\)", "<a href=\"$2\">$1</a>");

        return result;
    }

    // ---------- helpers ----------
    private static string[] SplitTableRow(string line)
    {
        return line.Trim().Trim('|').Split('|').Select(c => c.Trim()).ToArray();
    }

    private static bool IsTableSeparator(string line)
    {
        var t = line.Trim();
        return t.Contains('-') && Regex.IsMatch(t, @"^\|?[\s\-\|:]+\|?$");
    }

    private static string Slugify(string text)
    {
        var cleaned = Regex.Replace(text.ToLowerInvariant(), @"[^a-z0-9\s\-]", "");
        return Regex.Replace(cleaned.Trim(), @"\s+", "-");
    }
}