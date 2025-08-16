namespace SearchEngine_.utils;

/// <summary>
/// Implementation of ILinkResolver for downloading documents and obtaining MIME types.
/// </summary>
public class LinkResolver : ILinkResolver
{
    private readonly HttpClient _httpClient;
    
    public LinkResolver()
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(5); // 5 minute timeout
    }
    
    public StreamReader ResolveLink(string link, List<string> acceptableMimeTypes)
    {
        try
        {
            var response = _httpClient.GetAsync(link).Result;
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to download document: {response.StatusCode}");
            }
            
            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (string.IsNullOrEmpty(contentType))
            {
                contentType = "text/plain"; // Default to text if no content type specified
            }
            
            // Check if content type is acceptable
            if (!acceptableMimeTypes.Contains(contentType))
            {
                throw new UnsupportedMediaTypeException($"Content type '{contentType}' is not supported. Acceptable types: {string.Join(", ", acceptableMimeTypes)}");
            }
            
            var stream = response.Content.ReadAsStreamAsync().Result;
            return new StreamReader(stream);
        }
        catch (Exception ex) when (ex is not UnsupportedMediaTypeException)
        {
            throw new HttpRequestException($"Failed to resolve link '{link}': {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Exception thrown when the media type is not supported.
/// </summary>
public class UnsupportedMediaTypeException : Exception
{
    public UnsupportedMediaTypeException(string message) : base(message) { }
}
