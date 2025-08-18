using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.Text;
using System.Text.RegularExpressions;

namespace SearchEngine_.ReadableDocuments;

/// <summary>
/// Implementation of IReadableDocument for PDF documents using iTextSharp.
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
        try
        {
            // Use iText7 to extract text from PDF
            using (var pdfReader = new PdfReader(_reader.BaseStream))
            using (var pdfDocument = new PdfDocument(pdfReader))
            {
                var text = new StringBuilder();
                
                for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                {
                    var page = pdfDocument.GetPage(i);
                    var strategy = new SimpleTextExtractionStrategy();
                    var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                    text.AppendLine(pageText);
                }
                
                _content = text.ToString();
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

