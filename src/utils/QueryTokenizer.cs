using SearchEngine_.models;

namespace SearchEngine_.utils;

/// <summary>
/// Implementation of IQueryTokenizer for processing search queries.
/// </summary>
public class QueryTokenizer : IQueryTokenizer
{
    private readonly IStopWordFilter _stopWordFilter;
    
    public QueryTokenizer(IStopWordFilter stopWordFilter)
    {
        _stopWordFilter = stopWordFilter;
    }
    
    public List<Token> TokenizeQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<Token>();
        
        // Split query into words and convert to lowercase
        var words = query.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(word => word.ToLowerInvariant())
            .ToList();
        
        // Filter out stop words
        var filteredWords = _stopWordFilter.FilterOutStopWords(words);
        
        // Create tokens (in a real implementation, you would look up IDs from database)
        var tokens = new List<Token>();
        for (int i = 0; i < filteredWords.Count; i++)
        {
            tokens.Add(new Token
            {
                Id = i + 1, // Placeholder ID - in real implementation, get from database
                Word = filteredWords[i]
            });
        }
        
        return tokens;
    }
}
