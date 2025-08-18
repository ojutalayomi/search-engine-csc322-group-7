namespace SearchEngine_.ReadableDocuments;

/// <summary>
/// Abstract data type that allows words to be read from a document.
/// </summary>
public interface IReadableDocument
{
    /// <summary>
    /// Opens the document for reading.
    /// </summary>
    void OpenDocument();
    
    /// <summary>
    /// Reads the next word from the document.
    /// </summary>
    /// <returns>The next word as a string, or null if no more words.</returns>
    string? ReadNextWord();
    
    /// <summary>
    /// Gets the MIME type of the document.
    /// </summary>
    string MimeType { get; }
}
