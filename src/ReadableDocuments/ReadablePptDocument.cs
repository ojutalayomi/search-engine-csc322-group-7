using GemBox.Presentation;
using System.Text;
using System.Text.RegularExpressions;

namespace SearchEngine_.ReadableDocuments;

/// <summary>
/// Implementation of IReadableDocument for Microsoft PowerPoint PPT documents using GemBox.Presentation.
/// </summary>
public class ReadablePptDocument : IReadableDocument
{
    private readonly StreamReader _reader;
    private string _content = string.Empty;
    private readonly List<string> _words = [];
    private int _wordIndex = 0;
    
    public string MimeType => "application/vnd.ms-powerpoint";
    
    public ReadablePptDocument(StreamReader reader)
    {
        _reader = reader;
    }
    
    public void OpenDocument()
    {
        try
        {
            // Use GemBox.Presentation to extract text from PPT files
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");
            var presentation = PresentationDocument.Load(_reader.BaseStream);
            var text = new StringBuilder();
            
            foreach (var slide in presentation.Slides)
            {
                text.AppendLine(slide.Content.ToString());
            }
            
            _content = text.ToString();
            _reader.Close();
            
            // Process the extracted text
            var textContent = Regex.Replace(_content, @"[^\w\s]", " ");
            textContent = Regex.Replace(textContent, @"\s+", " ");
            
            _words.AddRange(textContent.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(word => !string.IsNullOrWhiteSpace(word))
                .Select(word => word.ToLowerInvariant()));
        }
        catch (Exception)
        {
            // If PPT processing fails, create empty content
            _content = string.Empty;
            _words.Clear();
        }
    }
    
    public string? ReadNextWord()
    {
        if (_wordIndex >= _words.Count)
            return null;
            
        return _words[_wordIndex++];
    }
}
