namespace SearchEngine_.ReadableDocuments;

/// <summary>
/// Implementation of IReadableDocument for plain text documents.
/// </summary>
public class ReadableTextDocument : IReadableDocument
{
    private readonly StreamReader _reader;
    private string _content = string.Empty;
    private readonly List<string> _words = new();
    private int _wordIndex = 0;
    
    public string MimeType => "text/plain";
    
    public ReadableTextDocument(StreamReader reader)
    {
        _reader = reader;
    }
    
    public void OpenDocument()
    {
        _content = _reader.ReadToEnd();
        _reader.Close();
        
        // Split content into words, removing punctuation and converting to lowercase
        var words = _content.Split(new[] { ' ', '\t', '\n', '\r', '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']', '{', '}' }, 
            StringSplitOptions.RemoveEmptyEntries);
            
        _words.AddRange(words.Where(word => !string.IsNullOrWhiteSpace(word))
            .Select(word => word.ToLowerInvariant()));
    }
    
    public string? ReadNextWord()
    {
        if (_wordIndex >= _words.Count)
            return null;
            
        return _words[_wordIndex++];
    }
}
