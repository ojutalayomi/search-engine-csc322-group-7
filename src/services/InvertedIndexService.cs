using System.Collections.Concurrent;

namespace SearchEngine.services;

/// <summary>
/// High-performance inverted index service for fast document retrieval.
/// Implements the inverted index data structure to achieve sub-10ms query response times.
/// </summary>
public class InvertedIndexService
{
    // Inverted index: word -> List of (documentId, frequency, positions)
    private readonly ConcurrentDictionary<string, List<DocumentOccurrence>> _invertedIndex;
    
    // Document metadata cache
    private readonly ConcurrentDictionary<int, DocumentMetadata> _documentMetadata;
    
    // Word frequency cache for ranking
    private readonly ConcurrentDictionary<string, int> _wordFrequency;
    
    private int _nextDocumentId = 1;
    
    public InvertedIndexService()
    {
        _invertedIndex = new ConcurrentDictionary<string, List<DocumentOccurrence>>();
        _documentMetadata = new ConcurrentDictionary<int, DocumentMetadata>();
        _wordFrequency = new ConcurrentDictionary<string, int>();
    }
    
    /// <summary>
    /// Indexes a document and adds it to the inverted index.
    /// </summary>
    /// <param name="documentLink">The document URL/link</param>
    /// <param name="frequencyDict">Word frequency dictionary</param>
    /// <returns>The assigned document ID</returns>
    public int IndexDocument(string documentLink, IDictionary<string, long> frequencyDict)
    {
        var documentId = Interlocked.Increment(ref _nextDocumentId);
        
        // Store document metadata
        _documentMetadata[documentId] = new DocumentMetadata
        {
            Id = documentId,
            Link = documentLink,
            IndexedAt = DateTime.UtcNow,
            WordCount = (int)frequencyDict.Values.Sum()
        };
        
        // Add to inverted index
        foreach (var kvp in frequencyDict)
        {
            var word = kvp.Key;
            var frequency = kvp.Value;
            
            var occurrence = new DocumentOccurrence
            {
                DocumentId = documentId,
                Frequency = (int)frequency,
                Score = CalculateWordScore(word, (int)frequency)
            };
            
            _invertedIndex.AddOrUpdate(word, 
                new List<DocumentOccurrence> { occurrence },
                (key, existing) =>
                {
                    existing.Add(occurrence);
                    return existing;
                });
            
            // Update global word frequency
            _wordFrequency.AddOrUpdate(word, 1, (key, count) => count + 1);
        }
        
        return documentId;
    }
    
    /// <summary>
    /// Searches for documents containing the specified words.
    /// Optimized for sub-10ms response times.
    /// </summary>
    /// <param name="words">List of words to search for</param>
    /// <returns>Ranked list of search results</returns>
    public List<SearchResult> SearchDocuments(List<string> words)
    {
        var startTime = DateTime.UtcNow;
        
        if (!words.Any())
            return new List<SearchResult>();
        
        // Find documents containing any of the search words
        var documentScores = new Dictionary<int, double>();
        
        foreach (var word in words)
        {
            if (_invertedIndex.TryGetValue(word, out var occurrences))
            {
                foreach (var occurrence in occurrences)
                {
                    if (!documentScores.ContainsKey(occurrence.DocumentId))
                        documentScores[occurrence.DocumentId] = 0;
                    
                    // TF-IDF scoring
                    var tf = occurrence.Frequency;
                    var idf = Math.Log((double)_documentMetadata.Count / _wordFrequency.GetValueOrDefault(word, 1));
                    var score = tf * idf * occurrence.Score;
                    
                    documentScores[occurrence.DocumentId] += score;
                }
            }
        }
        
        // Rank documents by score
        var rankedResults = documentScores
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => CreateSearchResult(kvp.Key, kvp.Value))
            .Where(result => result != null)
            .Cast<SearchResult>()
            .ToList();
        
        var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
        
        // Log performance metrics
        if (responseTime > 10) // Warning if response time exceeds 10ms
        {
            Console.WriteLine($"Warning: Search query took {responseTime:F2}ms (target: <10ms)");
        }
        
        return rankedResults;
    }
    
    /// <summary>
    /// Gets auto-complete suggestions for a partial word.
    /// </summary>
    /// <param name="partialWord">Partial word to complete</param>
    /// <param name="maxSuggestions">Maximum number of suggestions</param>
    /// <returns>List of word suggestions</returns>
    public List<string> GetAutoCompleteSuggestions(string partialWord, int maxSuggestions = 10)
    {
        if (string.IsNullOrWhiteSpace(partialWord))
            return new List<string>();
        
        var suggestions = _invertedIndex.Keys
            .Where(word => word.StartsWith(partialWord, StringComparison.OrdinalIgnoreCase))
            .OrderBy(word => word.Length) // Prefer shorter words
            .ThenBy(word => word)
            .Take(maxSuggestions)
            .ToList();
        
        return suggestions;
    }
    
    /// <summary>
    /// Gets document metadata by ID.
    /// </summary>
    public DocumentMetadata? GetDocumentMetadata(int documentId)
    {
        return _documentMetadata.TryGetValue(documentId, out var metadata) ? metadata : null;
    }
    
    /// <summary>
    /// Gets index statistics.
    /// </summary>
    public IndexStatistics GetStatistics()
    {
        return new IndexStatistics
        {
            TotalDocuments = _documentMetadata.Count,
            TotalWords = _invertedIndex.Count,
            TotalOccurrences = _invertedIndex.Values.Sum(list => list.Count),
            AverageWordsPerDocument = _documentMetadata.Values.Average(doc => doc.WordCount)
        };
    }
    
    private double CalculateWordScore(string word, int frequency)
    {
        // Simple scoring algorithm - can be enhanced with more sophisticated approaches
        var baseScore = Math.Log(frequency + 1);
        
        // Boost for longer words (more specific)
        var lengthBonus = Math.Min(word.Length / 10.0, 1.0);
        
        return baseScore * (1 + lengthBonus);
    }
    
    private SearchResult? CreateSearchResult(int documentId, double score)
    {
        if (!_documentMetadata.TryGetValue(documentId, out var metadata))
            return null;
        
        return new SearchResult
        {
            DocumentLink = metadata.Link,
            DocumentName = $"Document {documentId}",
            RelevanceScore = Math.Min(score / 100.0, 1.0), // Normalize to 0-1 range
            Snippet = $"Document containing relevant content (Score: {score:F2})",
            DocumentId = documentId,
            IndexedAt = metadata.IndexedAt
        };
    }
}

/// <summary>
/// Represents a document occurrence in the inverted index.
/// </summary>
public class DocumentOccurrence
{
    public int DocumentId { get; set; }
    public int Frequency { get; set; }
    public double Score { get; set; }
}

/// <summary>
/// Document metadata for ranking and display.
/// </summary>
public class DocumentMetadata
{
    public int Id { get; set; }
    public string Link { get; set; } = string.Empty;
    public DateTime IndexedAt { get; set; }
    public int WordCount { get; set; }
}

/// <summary>
/// Inverted index statistics.
/// </summary>
public class IndexStatistics
{
    public int TotalDocuments { get; set; }
    public int TotalWords { get; set; }
    public int TotalOccurrences { get; set; }
    public double AverageWordsPerDocument { get; set; }
}
