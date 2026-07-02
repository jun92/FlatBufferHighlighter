using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Language.StandardClassification;

namespace FlatBufferHighlighter
{
    internal class FlatBufferClassifier : IClassifier
    {
        private readonly IClassificationType _keywordType;
        private readonly IClassificationType _commentType;
        private readonly IClassificationType _stringType;
        private readonly IClassificationType _typeType;
        private readonly IClassificationType _numberType;

        private static readonly HashSet<string> Keywords = new HashSet<string>(StringComparer.Ordinal)
        {
            "namespace", "attribute", "table", "struct", "enum", "union",
            "root_type", "file_identifier", "file_extension", "rpc_service",
            "rpc", "include"
        };

        private static readonly HashSet<string> Types = new HashSet<string>(StringComparer.Ordinal)
        {
            "bool", "byte", "ubyte", "short", "ushort", "int", "uint",
            "float", "double", "long", "ulong", "string", "hash",
            "int8", "uint8", "int16", "uint16", "int32", "uint32",
            "int64", "uint64", "float32", "float64"
        };

        private static readonly HashSet<string> Literals = new HashSet<string>(StringComparer.Ordinal)
        {
            "true", "false", "null"
        };

        private static readonly Regex TokenRegex = new Regex(
            @"(?<comment>//.*)|(?<string>""(?:[^""\\]|\\.)*"")|(?<number>\b0x[0-9a-fA-F]+\b|\b\d+(?:\.\d+)?\b)|(?<word>\b[a-zA-Z_][a-zA-Z0-9_]*\b)",
            RegexOptions.Compiled);

        public FlatBufferClassifier(IClassificationTypeRegistryService registry)
        {
            _keywordType = registry.GetClassificationType(PredefinedClassificationTypeNames.Keyword);
            _commentType = registry.GetClassificationType(PredefinedClassificationTypeNames.Comment);
            _stringType = registry.GetClassificationType(PredefinedClassificationTypeNames.String);
            _typeType = registry.GetClassificationType(PredefinedClassificationTypeNames.Type);
            _numberType = registry.GetClassificationType(PredefinedClassificationTypeNames.Number);
        }

#pragma warning disable CS0067 // Event is never used
        public event EventHandler<ClassificationChangedEventArgs>? ClassificationChanged;
#pragma warning restore CS0067

        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            var classifications = new List<ClassificationSpan>();
            var snapshot = span.Snapshot;
            int startLine = snapshot.GetLineNumberFromPosition(span.Start);
            int endLine = snapshot.GetLineNumberFromPosition(span.End);

            for (int i = startLine; i <= endLine; i++)
            {
                var line = snapshot.GetLineFromLineNumber(i);
                string text = line.GetText();
                var matches = TokenRegex.Matches(text);

                foreach (Match match in matches)
                {
                    IClassificationType? type = null;

                    if (match.Groups["comment"].Success)
                    {
                        type = _commentType;
                    }
                    else if (match.Groups["string"].Success)
                    {
                        type = _stringType;
                    }
                    else if (match.Groups["number"].Success)
                    {
                        type = _numberType;
                    }
                    else if (match.Groups["word"].Success)
                    {
                        string word = match.Value;
                        if (Keywords.Contains(word) || Literals.Contains(word))
                        {
                            type = _keywordType;
                        }
                        else if (Types.Contains(word))
                        {
                            type = _typeType;
                        }
                    }

                    if (type != null)
                    {
                        var tokenSpan = new SnapshotSpan(snapshot, line.Start + match.Index, match.Length);
                        classifications.Add(new ClassificationSpan(tokenSpan, type));
                    }
                }
            }

            return classifications;
        }
    }
}
