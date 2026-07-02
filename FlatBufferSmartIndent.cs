using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace FlatBufferHighlighter
{
    internal class FlatBufferSmartIndent : ISmartIndent
    {
        private readonly ITextView _textView;

        public FlatBufferSmartIndent(ITextView textView)
        {
            _textView = textView;
        }

        public int? GetDesiredIndentation(ITextSnapshotLine line)
        {
            var snapshot = line.Snapshot;
            int currentLineNumber = line.LineNumber;

            // Retrieve editor tab size options
            int tabSize = _textView.Options.GetOptionValue(DefaultOptions.TabSizeOptionId);

            // Check if the current line starts with '}'
            string currentText = line.GetText().TrimStart();
            if (currentText.StartsWith("}"))
            {
                // Scan backwards to find the matching '{'
                int braceCount = 1;
                for (int i = currentLineNumber - 1; i >= 0; i--)
                {
                    var prevLine = snapshot.GetLineFromLineNumber(i);
                    string prevText = prevLine.GetText();
                    GetBraceCounts(prevText, out int openBraces, out int closeBraces);

                    braceCount += closeBraces;
                    braceCount -= openBraces;

                    if (braceCount <= 0)
                    {
                        return GetLineIndentation(prevText, tabSize);
                    }
                }
            }

            // For any other line (usually empty new lines), determine indentation based on the previous non-empty line
            ITextSnapshotLine? prevNonEmptyLine = null;
            for (int i = currentLineNumber - 1; i >= 0; i--)
            {
                var prevLine = snapshot.GetLineFromLineNumber(i);
                if (!string.IsNullOrWhiteSpace(prevLine.GetText()))
                {
                    prevNonEmptyLine = prevLine;
                    break;
                }
            }

            if (prevNonEmptyLine == null)
            {
                return 0;
            }

            string prevTextClean = prevNonEmptyLine.GetText();
            GetBraceCounts(prevTextClean, out int prevOpenBraces, out int prevCloseBraces);

            int prevIndent = GetLineIndentation(prevTextClean, tabSize);

            if (prevOpenBraces > prevCloseBraces)
            {
                return prevIndent + tabSize;
            }

            return prevIndent;
        }

        private static int GetLineIndentation(string text, int tabSize)
        {
            int indent = 0;
            foreach (char c in text)
            {
                if (c == ' ')
                {
                    indent++;
                }
                else if (c == '\t')
                {
                    indent += tabSize;
                }
                else
                {
                    break;
                }
            }
            return indent;
        }

        private static void GetBraceCounts(string text, out int openBraces, out int closeBraces)
        {
            openBraces = 0;
            closeBraces = 0;
            bool inString = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (inString)
                {
                    if (c == '"')
                    {
                        // Check if escaped
                        if (i > 0 && text[i - 1] == '\\')
                        {
                            continue;
                        }
                        inString = false;
                    }
                }
                else
                {
                    if (c == '"')
                    {
                        inString = true;
                    }
                    else if (c == '/' && i + 1 < text.Length && text[i + 1] == '/')
                    {
                        // Line comment starts, ignore the rest of the line
                        break;
                    }
                    else if (c == '{')
                    {
                        openBraces++;
                    }
                    else if (c == '}')
                    {
                        closeBraces++;
                    }
                }
            }
        }

        public void Dispose()
        {
        }
    }
}
