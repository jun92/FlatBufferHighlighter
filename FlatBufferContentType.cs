using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace FlatBufferHighlighter
{
    public static class FlatBufferContentType
    {
        [Export]
        [Name("flatbuffer")]
        [BaseDefinition("code")]
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public static ContentTypeDefinition FlatBufferContentTypeDefinition;

        [Export]
        [FileExtension(".fbs")]
        [ContentType("flatbuffer")]
        public static FileExtensionToContentTypeDefinition FlatBufferFileExtensionDefinition;
#pragma warning restore CS8618
    }
}
