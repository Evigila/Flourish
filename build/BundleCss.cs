using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

/// <summary>Expands local CSS imports without changing cascade order or fetching external resources.</summary>
public sealed class BundleCss : Task
{
    [Required] public string SourceRoot { get; set; }
    [Required] public string EntryFile { get; set; }
    [Required] public string OutputFile { get; set; }

    private string root;
    private string entryDirectory;
    private static readonly StringComparison PathComparison = Path.DirectorySeparatorChar == (char)92 ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
    private readonly HashSet<string> active = new HashSet<string>(Path.DirectorySeparatorChar == (char)92 ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);

    public override bool Execute()
    {
        try
        {
            root = Path.GetFullPath(SourceRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!Directory.Exists(root) || (File.GetAttributes(root) & FileAttributes.ReparsePoint) != 0) throw new InvalidOperationException("CSS source root must be an existing ordinary directory: " + SourceRoot);
            string entry = Contained(EntryFile);
            entryDirectory = Path.GetDirectoryName(entry);
            string output = "/* Generated from modular CSS sources. Do not edit. */\n" + Expand(entry);
            output = output.Replace("\r\n", "\n").Replace("\r", "\n");
            // Compare content on every build: edits, removed imports and deleted inputs must not be hidden by timestamp-only incremental checks.
            if (!File.Exists(OutputFile) || File.ReadAllText(OutputFile) != output)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(OutputFile)));
                File.WriteAllText(OutputFile, output, new UTF8Encoding(false));
                Log.LogMessage(MessageImportance.Low, "Bundled CSS: {0}", OutputFile);
            }
            return true;
        }
        catch (Exception exception)
        {
            Log.LogError("CSS bundle failed: {0}", exception.Message);
            return false;
        }
    }

    private string Contained(string path)
    {
        string full = Path.GetFullPath(path);
        if (!full.StartsWith(root, PathComparison))
            throw new InvalidOperationException("Path leaves the CSS source root: " + path);
        // A lexical containment check alone is insufficient for a linked directory.
        for (string current = full; !String.Equals(current, root.TrimEnd(Path.DirectorySeparatorChar), PathComparison); current = Path.GetDirectoryName(current))
        {
            if ((File.Exists(current) || Directory.Exists(current)) && (File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidOperationException("Linked CSS inputs are unsupported: " + current);
        }
        return full;
    }

    private string Expand(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Missing CSS input: " + path, path);
        if (!active.Add(path)) throw new InvalidOperationException("CSS import cycle at " + path);
        try
        {
            string css = File.ReadAllText(path);
            var output = new StringBuilder();
            int position = 0;
            int depth = 0;
            bool importsAllowed = true;
            while (position < css.Length)
            {
                if (Starts(css, position, "/*"))
                {
                    int end = css.IndexOf("*/", position + 2, StringComparison.Ordinal);
                    if (end < 0) throw Invalid(path, "Unterminated comment");
                    output.Append(css, position, end + 2 - position);
                    position = end + 2;
                }
                else if (css[position] == '"' || css[position] == '\'')
                {
                    int end = QuotedEnd(css, position);
                    output.Append(css, position, end - position);
                    position = end;
                    if (depth == 0) importsAllowed = false;
                }
                else if (Token(css, position, "@import"))
                {
                    if (depth != 0 || !importsAllowed) throw Invalid(path, "@import must precede style rules");
                    int end = StatementEnd(css, position + 7);
                    string body = css.Substring(position + 7, end - position - 7);
                    int consumed;
                    string import = ReadReference(body, out consumed);
                    string condition = StripComments(body.Substring(consumed)).Trim();
                    if (condition.Length > 0)
                        throw Invalid(path, "Conditional @import is unsupported; express media/supports/layer inside the imported stylesheet: " + condition);
                    if (IsExternal(import) || import.StartsWith("#", StringComparison.Ordinal) || import.StartsWith("?", StringComparison.Ordinal))
                        throw Invalid(path, "Only local unconditional CSS imports are supported: " + import);
                    if (import.IndexOfAny(new[] { '?', '#' }) >= 0) throw Invalid(path, "CSS import query/fragment is unsupported: " + import);
                    string imported = Resolve(path, import);
                    output.Append("\n").Append(Expand(imported)).Append("\n");
                    position = end + 1;
                }
                else if (Token(css, position, "@charset"))
                {
                    int end = StatementEnd(css, position + 8);
                    if (!importsAllowed || depth != 0 || !Regex.IsMatch(StripComments(css.Substring(position + 8, end - position - 8)).Trim(), "^[\"']UTF-8[\"']$", RegexOptions.IgnoreCase))
                        throw Invalid(path, "Only a leading UTF-8 @charset is supported");
                    position = end + 1; // The complete bundle is written as UTF-8 without a BOM.
                }
                else if (Token(css, position, "url") && NextNonSpace(css, position + 3) < css.Length && css[NextNonSpace(css, position + 3)] == '(')
                {
                    int start = NextNonSpace(css, position + 3);
                    int end = UrlEnd(css, start);
                    string reference = ReadUrl(css.Substring(start + 1, end - start - 1));
                    if (reference.Length == 0 || IsExternal(reference) || reference[0] == '#' || reference[0] == '?')
                        output.Append(css, position, end + 1 - position);
                    else
                    {
                        int suffixIndex = reference.IndexOfAny(new[] { '?', '#' });
                        string relative = suffixIndex < 0 ? reference : reference.Substring(0, suffixIndex);
                        string suffix = suffixIndex < 0 ? "" : reference.Substring(suffixIndex);
                        string resource = Resolve(path, relative);
                        if (!File.Exists(resource)) throw Invalid(path, "Missing relative CSS resource: " + reference);
                        string rebased = RelativePath(entryDirectory, resource).Replace('\\', '/') + suffix;
                        output.Append("url(\"").Append(rebased.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append("\")");
                    }
                    position = end + 1;
                    if (depth == 0) importsAllowed = false;
                }
                else
                {
                    char value = css[position++];
                    output.Append(value);
                    if (value == '{') depth++;
                    else if (value == '}') depth--;
                    if (depth < 0) throw Invalid(path, "Unbalanced closing brace");
                    if (depth == 0 && !Char.IsWhiteSpace(value)) importsAllowed = false;
                }
            }
            if (depth != 0) throw Invalid(path, "Unbalanced braces");
            return output.ToString();
        }
        finally { active.Remove(path); }
    }

    private string Resolve(string containingFile, string reference)
    {
        if (reference.IndexOf('\\') >= 0) throw Invalid(containingFile, "Escaped/backslash local paths are unsupported: " + reference);
        string decoded = Uri.UnescapeDataString(reference);
        return Contained(Path.Combine(Path.GetDirectoryName(containingFile), decoded));
    }

    private static string RelativePath(string directory, string file)
    {
        // URI-based relative paths also work with the reference assemblies supplied to inline MSBuild tasks.
        var baseUri = new Uri(Path.GetFullPath(directory).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
        return baseUri.MakeRelativeUri(new Uri(Path.GetFullPath(file))).ToString();
    }

    private static bool IsExternal(string reference) =>
        reference.StartsWith("/", StringComparison.Ordinal) || Regex.IsMatch(reference, @"^[a-z][a-z0-9+.-]*:", RegexOptions.IgnoreCase);

    private static Exception Invalid(string path, string reason) => new InvalidOperationException(path + ": " + reason);
    private static bool Starts(string text, int position, string value) => String.Compare(text, position, value, 0, value.Length, StringComparison.OrdinalIgnoreCase) == 0;
    private static bool Token(string text, int position, string token) =>
        position + token.Length <= text.Length && (position == 0 || !Identifier(text[position - 1])) &&
        Starts(text, position, token) && (position + token.Length == text.Length || !Identifier(text[position + token.Length]));
    private static bool Identifier(char value) => Char.IsLetterOrDigit(value) || value == '_' || value == '-' || value == '\\';
    private static int NextNonSpace(string text, int position) { while (position < text.Length && Char.IsWhiteSpace(text[position])) position++; return position; }
    private static string StripComments(string text) => Regex.Replace(text, @"/\*[\s\S]*?\*/", " ");

    private static int QuotedEnd(string text, int position)
    {
        char quote = text[position++];
        while (position < text.Length)
        {
            if (text[position] == '\\') { position += 2; continue; }
            if (text[position++] == quote) return position;
        }
        throw new InvalidOperationException("Unterminated CSS string");
    }

    private static int StatementEnd(string text, int position)
    {
        int parentheses = 0;
        while (position < text.Length)
        {
            if (Starts(text, position, "/*"))
            {
                int end = text.IndexOf("*/", position + 2, StringComparison.Ordinal);
                if (end < 0) throw new InvalidOperationException("Unterminated CSS comment");
                position = end + 2;
            }
            else if (text[position] == '"' || text[position] == '\'') position = QuotedEnd(text, position);
            else
            {
                char value = text[position];
                if (value == '(') parentheses++;
                if (value == ')') parentheses--;
                if (value == ';' && parentheses == 0) return position;
                position++;
            }
        }
        throw new InvalidOperationException("Unterminated @import or @charset");
    }

    private static int UrlEnd(string text, int start)
    {
        int position = start + 1;
        while (position < text.Length)
        {
            if (text[position] == '"' || text[position] == '\'') position = QuotedEnd(text, position);
            else if (text[position] == '\\') position += 2;
            else if (text[position] == ')') return position;
            else position++;
        }
        throw new InvalidOperationException("Unterminated CSS url()");
    }

    private static string ReadUrl(string text)
    {
        text = text.Trim();
        if (text.Length > 0 && (text[0] == '"' || text[0] == '\''))
        {
            int end = QuotedEnd(text, 0);
            if (end != text.Length) throw new InvalidOperationException("Unexpected trailing content in CSS url()");
            return text.Substring(1, text.Length - 2);
        }
        return text;
    }

    private static string ReadReference(string text, out int consumed)
    {
        int start = NextNonSpace(text, 0);
        // Comments between @import and its reference are valid.
        while (Starts(text, start, "/*"))
        {
            int end = text.IndexOf("*/", start + 2, StringComparison.Ordinal);
            if (end < 0) throw new InvalidOperationException("Unterminated CSS import comment");
            start = NextNonSpace(text, end + 2);
        }
        if (start < text.Length && (text[start] == '"' || text[start] == '\''))
        {
            consumed = QuotedEnd(text, start);
            return text.Substring(start + 1, consumed - start - 2);
        }
        if (Token(text, start, "url"))
        {
            int opening = NextNonSpace(text, start + 3);
            if (opening < text.Length && text[opening] == '(')
            {
                int end = UrlEnd(text, opening);
                consumed = end + 1;
                return ReadUrl(text.Substring(opening + 1, end - opening - 1));
            }
        }
        throw new InvalidOperationException("@import requires a quoted local path or url()");
    }
}

