namespace SearchEngine_.ReadableDocuments;

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
            { "application/msword", reader => new ReadableDocDocument(reader) }, // DOC
            { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", reader => new ReadableDocxDocument(reader) }, // DOCX
            { "application/vnd.ms-powerpoint", reader => new ReadablePptDocument(reader) }, // PPT
            { "application/vnd.openxmlformats-officedocument.presentationml.presentation", reader => new ReadablePptxDocument(reader) }, // PPTX
            { "application/vnd.ms-excel", reader => new ReadableXlsDocument(reader) }, // XLS
            { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", reader => new ReadableXlsxDocument(reader) } // XLSX
        };
    }
    
    public IReadableDocument CreateReadableDocument(string mimeType, StreamReader reader)
    {
        return _documentCreators.TryGetValue(mimeType, out var creator) ? creator(reader) :
            // Default to a text document if the MIME type is not supported
            new ReadableTextDocument(reader);
    }
}
