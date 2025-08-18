using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using System.Text;
using System.Text.RegularExpressions;

namespace SearchEngine_.ReadableDocuments;

/// <summary>
/// Implementation of IReadableDocument for Microsoft PowerPoint PPTX documents using DocumentFormat.OpenXml.
/// </summary>
public class ReadablePptxDocument : IReadableDocument
{
    private readonly StreamReader _reader;
    private string _content = string.Empty;
    private readonly List<string> _words = new();
    private int _wordIndex = 0;
    
    public string MimeType => "application/vnd.openxmlformats-officedocument.presentationml.presentation";
    
    public ReadablePptxDocument(StreamReader reader)
    {
        _reader = reader;
    }
    
    public void OpenDocument()
    {
        try
        {
            // Use DocumentFormat.OpenXml to extract text from PPTX files
            using (var ppt = PresentationDocument.Open(_reader.BaseStream, false))
            {
                var presentationPart = ppt.PresentationPart;
                if (presentationPart?.Presentation != null)
                {
                    var text = new StringBuilder();
                    var slides = presentationPart.Presentation.SlideIdList?.ChildElements;
                    
                    if (slides != null)
                    {
                        foreach (var slideId in slides.OfType<SlideId>())
                        {
                            if (slideId.RelationshipId?.Value != null)
                            {
                                var slidePart = presentationPart.GetPartById(slideId.RelationshipId.Value) as SlidePart;
                                if (slidePart?.Slide != null)
                                {
                                    text.AppendLine(slidePart.Slide.InnerText);
                                }
                            }
                        }
                    }
                    
                    _content = text.ToString();
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
            // If PPTX processing fails, create empty content
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
