using DocumentFormat.OpenXml.Packaging;
using System.Text.RegularExpressions;

namespace SearchEngine_.ReadableDocuments;

/// <summary>
/// Implementation of IReadableDocument for Microsoft Word DOCX documents using DocumentFormat.OpenXml.
/// </summary>
public class ReadableDocxDocument : IReadableDocument
{
    private readonly StreamReader _reader;
    private string _content = string.Empty;
    private readonly List<string> _words = new();
    private int _wordIndex = 0;
    
    public string MimeType => "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    
    public ReadableDocxDocument(StreamReader reader)
    {
        _reader = reader;
    }
    
    public void OpenDocument()
    {
        try
        {
            // Use DocumentFormat.OpenXml to extract text from DOCX files
            using (var doc = WordprocessingDocument.Open(_reader.BaseStream, false))
            {
                var body = doc.MainDocumentPart?.Document?.Body;
                if (body != null)
                {
                    _content = body.InnerText;
                }
            }
            
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
            // If DOCX processing fails, create empty content
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
