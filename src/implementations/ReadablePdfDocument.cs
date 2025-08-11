using SearchEngine.interfaces;
using System.Text.RegularExpressions;

namespace SearchEngine.implementations;

/// <summary>
/// Implementation of IReadableDocument for PDF documents.
/// Note: This is a simplified implementation. In production, use a proper PDF library like iTextSharp or PdfSharp.
/// </summary>
public class ReadablePdfDocument : IReadableDocument
{
    private readonly StreamReader _reader;
    private string _content = string.Empty;
    private readonly List<string> _words = new();
    private int _wordIndex = 0;
    
    public string MimeType => "application/pdf";
    
    public ReadablePdfDocument(StreamReader reader)
    {
        _reader = reader;
    }
    
    public void OpenDocument()
    {
        // Note: This is a simplified implementation
        // In a real implementation, you would use a PDF library to extract text
        // For now, we'll simulate PDF text extraction
        
        try
        {
            // Simulate PDF text extraction
            _content = "PDF document content would be extracted here using a proper PDF library.";
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
            // If PDF processing fails, create empty content
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
