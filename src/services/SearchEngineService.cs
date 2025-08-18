using SearchEngine_.indexing.models;
using SearchEngine_.ReadableDocuments;
using SearchEngine_.models;
using SearchEngine_.utils;

namespace SearchEngine.services;

/// <summary>
/// Main service that orchestrates the search engine operations.
/// </summary>
public class SearchEngineService
{
    private readonly ILinkResolver _linkResolver;
    private readonly IReadableDocumentFactory _documentFactory;
    private readonly IStopWordFilter _stopWordFilter;
    private readonly IQueryTokenizer _queryTokenizer;
    private readonly InvertedIndexService _indexService;
    
    public SearchEngineService(
        ILinkResolver linkResolver,
        IReadableDocumentFactory documentFactory,
        IStopWordFilter stopWordFilter,
        IQueryTokenizer queryTokenizer,
        InvertedIndexService indexService)
    {
        _linkResolver = linkResolver;
        _documentFactory = documentFactory;
        _stopWordFilter = stopWordFilter;
        _queryTokenizer = queryTokenizer;
        _indexService = indexService;
    }
    
    /// <summary>
    /// Processes a queue request by downloading, reading, and indexing the document.
    /// </summary>
    /// <param name="request">The queue request to process.</param>
    /// <returns>The computed index for the document.</returns>
    public async Task<DocumentIndex> ProcessQueueRequestAsync(QueueRequest request)
    {
        try
        {
            // Define acceptable MIME types
            var acceptableMimeTypes = new List<string> { 
                "text/html", 
                "text/plain", 
                "application/xml", 
                "application/pdf",
                "application/msword",
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "application/vnd.ms-powerpoint",
                "application/vnd.openxmlformats-officedocument.presentationml.presentation",
                "application/vnd.ms-excel",
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            };
            
            // Download the document
            var (contentType, streamReader) = _linkResolver.ResolveLink(request.DocumentURL, acceptableMimeTypes);
            
            // Create readable document based on MIME type
            var document = _documentFactory.CreateReadableDocument(contentType, streamReader);
            document.OpenDocument();
            
            // Read all words from the document
            var words = new List<string>();
            string? word;
            while ((word = document.ReadNextWord()) != null)
            {
                words.Add(word);
            }
            
            // Filter out stop words
            var filteredWords = _stopWordFilter.FilterOutStopWords(words);
            
            // Compute word frequencies
            var frequencyDict = filteredWords
                .GroupBy(w => w)
                .ToDictionary(g => g.Key, g => (long)g.Count());
            
            // Create and return the index
            return new DocumentIndex
            {
                Id = request.Id,
                DocumentType = contentType, // Use the actual detected MIME type
                DocumentLink = request.DocumentURL,
                FrequencyDict = frequencyDict,
                totalTermCount = words.Count
            };
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to process queue request for {request.DocumentURL}: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Searches for documents based on a query using the inverted index.
    /// </summary>
    /// <param name="query">The search query.</param>
    /// <returns>A list of search results.</returns>
    public async Task<List<SearchResult>> SearchAsync(string query)
    {
        // Tokenize the query
        var tokens = _queryTokenizer.TokenizeQuery(query);
        
        if (!tokens.Any())
            return new List<SearchResult>();
        
        // Extract words from tokens
        var words = tokens.Select(t => t.Word).ToList();
        
        // Use inverted index for fast search
        var results = _indexService.SearchDocuments(words);
        
        return results;
    }
    

}

/// <summary>
/// Represents a search result.
/// </summary>
public class SearchResult
{
    public string DocumentLink { get; set; } = string.Empty;
    public string DocumentName { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
    public string Snippet { get; set; } = string.Empty;
    public int DocumentId { get; set; }
    public DateTime IndexedAt { get; set; }
}
