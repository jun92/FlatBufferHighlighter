using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace FlatBufferHighlighter
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType("flatbuffer")]
    internal class FlatBufferSmartIndentProvider : ISmartIndentProvider
    {
        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            return new FlatBufferSmartIndent(textView);
        }
    }
}
