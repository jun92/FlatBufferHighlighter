using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace FlatBufferHighlighter
{
    [Export(typeof(IClassifierProvider))]
    [ContentType("flatbuffer")]
    internal class FlatBufferClassifierProvider : IClassifierProvider
    {
        [Import]
#pragma warning disable CS8618, CS0649
        internal IClassificationTypeRegistryService ClassificationRegistry = null!;
#pragma warning restore CS8618, CS0649

        public IClassifier GetClassifier(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty(() => new FlatBufferClassifier(ClassificationRegistry));
        }
    }
}
