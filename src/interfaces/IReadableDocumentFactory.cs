namespace SearchEngine.interfaces;

/// <summary>
/// Factory interface for creating readable documents based on MIME type.
/// </summary>
public interface IReadableDocumentFactory
{
    /// <summary>
    /// Creates a readable document based on the MIME type and stream reader.
    /// </summary>
    /// <param name="mimeType">The MIME type of the document.</param>
    /// <param name="reader">The stream reader containing the document data.</param>
    /// <returns>An implementation of IReadableDocument.</returns>
    IReadableDocument CreateReadableDocument(string mimeType, StreamReader reader);
}
