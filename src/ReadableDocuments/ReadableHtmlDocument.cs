using System.Text.RegularExpressions;

namespace SearchEngine_.ReadableDocuments;

/// <summary>
/// Implementation of IReadableDocument for HTML documents.
/// </summary>
public class ReadableHtmlDocument : IReadableDocument
{
    private readonly StreamReader _reader;
    private string _content = string.Empty;
    private readonly List<string> _words = new();
    private int _wordIndex = 0;
    
    public string MimeType => "text/html";
    
    public ReadableHtmlDocument(StreamReader reader)
    {
        _reader = reader;
    }
    
    public void OpenDocument()
    {
        _content = _reader.ReadToEnd();
        _reader.Close();
        
        // Extract text content from HTML, removing tags and special characters
        var textContent = Regex.Replace(_content, "<[^>]*>", " ");
        textContent = Regex.Replace(textContent, @"[^\w\s]", " ");
        textContent = Regex.Replace(textContent, @"\s+", " ");
        
        // Split into words and filter out empty strings
        _words.AddRange(textContent.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .Select(word => word.ToLowerInvariant()));
    }
    
    public string? ReadNextWord()
    {
        if (_wordIndex >= _words.Count)
            return null;
            
        return _words[_wordIndex++];
    }
}
