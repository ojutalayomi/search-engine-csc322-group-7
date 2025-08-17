using GemBox.Spreadsheet;
using System.Text;
using System.Text.RegularExpressions;

namespace SearchEngine_.ReadableDocuments;

/// <summary>
/// Implementation of IReadableDocument for Microsoft Excel XLS documents using GemBox.Spreadsheet.
/// </summary>
public class ReadableXlsDocument : IReadableDocument
{
    private readonly StreamReader _reader;
    private string _content = string.Empty;
    private readonly List<string> _words = new();
    private int _wordIndex = 0;
    
    public string MimeType => "application/vnd.ms-excel";
    
    public ReadableXlsDocument(StreamReader reader)
    {
        _reader = reader;
    }
    
    public void OpenDocument()
    {
        try
        {
            // Use GemBox.Spreadsheet to extract text from XLS files
            var workbook = ExcelFile.Load(_reader.BaseStream);
            var text = new StringBuilder();
            
            foreach (var worksheet in workbook.Worksheets)
            {
                foreach (var row in worksheet.Rows)
                {
                    foreach (var cell in row.AllocatedCells)
                    {
                        if (cell.Value != null)
                        {
                            text.Append(cell.Value.ToString() + " ");
                        }
                    }
                    text.AppendLine();
                }
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
            // If XLS processing fails, create empty content
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
