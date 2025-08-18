namespace SearchEngine_.utils;

/// <summary>
/// Responsible for downloading documents into memory and obtaining MIME types from response headers.
/// </summary>
public interface ILinkResolver
{
    /// <summary>
    /// Resolves a link by downloading the document and returning a stream reader.
    /// </summary>
    /// <param name="link">The URL to resolve.</param>
    /// <param name="acceptableMimeTypes">List of acceptable MIME types.</param>
    /// <returns>A tuple containing the content type and a stream reader containing the document data.</returns>
    /// <exception cref="UnsupportedMediaTypeException">Thrown when the content type doesn't match acceptable types.</exception>
    (string ContentType, StreamReader StreamReader) ResolveLink(string link, List<string> acceptableMimeTypes);
}
