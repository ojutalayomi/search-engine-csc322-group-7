namespace SearchEngine_.utils;

/// <summary>
/// Implementation of IStopWordFilter for removing common stop words from text.
/// </summary>
public class StopWordFilter : IStopWordFilter
{
    private readonly HashSet<string> _stopWords;
    
    public StopWordFilter()
    {
        // Common English stop words
        _stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "a", "an", "and", "are", "as", "at", "be", "by", "for", "from",
            "has", "he", "in", "is", "it", "its", "of", "on", "that", "the",
            "to", "was", "will", "with", "this", "but", "they", "have",
            "had", "what", "said", "each", "which", "she", "do", "how", "their",
            "if", "up", "out", "many", "then", "them", "these", "so", "some",
            "her", "would", "make", "like", "into", "him", "time", "two", "more",
            "go", "no", "way", "could", "my", "than", "first", "been", "call",
            "who", "its", "now", "find", "long", "down", "day", "did", "get",
            "come", "made", "may", "part"
        };
    }
    
    public List<string> FilterOutStopWords(List<string> words)
    {
        return words.Where(word => !_stopWords.Contains(word)).ToList();
    }
}
