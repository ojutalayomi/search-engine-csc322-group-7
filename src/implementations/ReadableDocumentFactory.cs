using SearchEngine.interfaces;

namespace SearchEngine.implementations;

/// <summary>
/// Factory implementation for creating readable documents based on MIME type.
/// </summary>
public class ReadableDocumentFactory : IReadableDocumentFactory
{
    private readonly Dictionary<string, Func<StreamReader, IReadableDocument>> _documentCreators;
    
    public ReadableDocumentFactory()
    {
        _documentCreators = new Dictionary<string, Func<StreamReader, IReadableDocument>>
        {
            { "text/html", reader => new ReadableHtmlDocument(reader) },
            { "text/plain", reader => new ReadableTextDocument(reader) },
            { "application/pdf", reader => new ReadablePdfDocument(reader) },
            { "application/xml", reader => new ReadableXmlDocument(reader) },
            { "application/msword", reader => new ReadableTextDocument(reader) }, // DOC
            { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", reader => new ReadableTextDocument(reader) }, // DOCX
            { "application/vnd.ms-powerpoint", reader => new ReadableTextDocument(reader) }, // PPT
            { "application/vnd.openxmlformats-officedocument.presentationml.presentation", reader => new ReadableTextDocument(reader) }, // PPTX
            { "application/vnd.ms-excel", reader => new ReadableTextDocument(reader) }, // XLS
            { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reader => new ReadableTextDocument(reader) } // XLSX
        };
    }
    
    public IReadableDocument CreateReadableDocument(string mimeType, StreamReader reader)
    {
        if (_documentCreators.TryGetValue(mimeType, out var creator))
        {
            return creator(reader);
        }
        
        // Default to text document if MIME type is not supported
        return new ReadableTextDocument(reader);
    }
}
