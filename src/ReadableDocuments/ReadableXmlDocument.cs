using System.Xml;

namespace SearchEngine_.ReadableDocuments;

/// <summary>
/// Implementation of IReadableDocument for XML documents.
/// </summary>
public class ReadableXmlDocument : IReadableDocument
{
    private readonly StreamReader _reader;
    private string _content = string.Empty;
    private readonly List<string> _words = new();
    private int _wordIndex = 0;
    
    public string MimeType => "application/xml";
    
    public ReadableXmlDocument(StreamReader reader)
    {
        _reader = reader;
    }
    
    public void OpenDocument()
    {
        try
        {
            // Read the XML content
            _content = _reader.ReadToEnd();
            _reader.Close();
            
            // Extract text content from XML, preserving some structure
            var textContent = ExtractTextFromXml(_content);
            
            // Process the extracted text
            var words = textContent.Split(new[] { ' ', '\t', '\n', '\r', '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']', '{', '}' }, 
                StringSplitOptions.RemoveEmptyEntries);
                
            _words.AddRange(words.Where(word => !string.IsNullOrWhiteSpace(word))
                .Select(word => word.ToLowerInvariant()));
        }
        catch (Exception)
        {
            // If XML processing fails, create empty content
            _content = string.Empty;
            _words.Clear();
        }
    }
    
    private string ExtractTextFromXml(string xmlContent)
    {
        try
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlContent);
            
            // Extract text from all text nodes
            var textNodes = xmlDoc.SelectNodes("//text()");
            var textContent = new List<string>();
            
            if (textNodes != null)
            {
                foreach (XmlNode node in textNodes)
                {
                    if (node.Value != null && !string.IsNullOrWhiteSpace(node.Value.Trim()))
                    {
                        textContent.Add(node.Value.Trim());
                    }
                }
            }
            
            return string.Join(" ", textContent);
        }
        catch
        {
            // Fallback: simple regex-based extraction
            var textContent = System.Text.RegularExpressions.Regex.Replace(xmlContent, @"<[^>]*>", " ");
            return System.Text.RegularExpressions.Regex.Replace(textContent, @"\s+", " ");
        }
    }
    
    public string? ReadNextWord()
    {
        if (_wordIndex >= _words.Count)
            return null;
            
        return _words[_wordIndex++];
    }
}
