using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace SearchEngine_.ReadableDocuments;

/// <summary>
/// Implementation of IReadableDocument for Microsoft Excel XLSX documents using DocumentFormat.OpenXml.
/// </summary>
public class ReadableXlsxDocument : IReadableDocument
{
    private readonly StreamReader _reader;
    private string _content = string.Empty;
    private readonly List<string> _words = new();
    private int _wordIndex = 0;
    
    public string MimeType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    
    public ReadableXlsxDocument(StreamReader reader)
    {
        _reader = reader;
    }
    
    public void OpenDocument()
    {
        try
        {
            // Use DocumentFormat.OpenXml to extract text from XLSX files
            using (var spreadsheet = SpreadsheetDocument.Open(_reader.BaseStream, false))
            {
                var workbookPart = spreadsheet.WorkbookPart;
                if (workbookPart?.Workbook?.Sheets != null)
                {
                    var text = new StringBuilder();
                    var sheets = workbookPart.Workbook.Sheets.Elements<Sheet>();
                    
                    foreach (var sheet in sheets)
                    {
                        if (sheet.Id?.Value != null)
                        {
                            var worksheetPart = workbookPart.GetPartById(sheet.Id.Value) as WorksheetPart;
                            if (worksheetPart?.Worksheet != null)
                            {
                                var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
                                if (sheetData != null)
                                {
                                    var rows = sheetData.Elements<Row>();
                                    foreach (var row in rows)
                                    {
                                        var cells = row.Elements<Cell>();
                                        foreach (var cell in cells)
                                        {
                                            if (cell.CellValue != null)
                                            {
                                                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                                                {
                                                    var stringTable = workbookPart.SharedStringTablePart?.SharedStringTable;
                                                    if (stringTable != null)
                                                    {
                                                        var sharedString = stringTable.Elements<SharedStringItem>().ElementAt(int.Parse(cell.CellValue.Text));
                                                        text.Append(sharedString.Text?.Text + " ");
                                                    }
                                                }
                                                else
                                                {
                                                    text.Append(cell.CellValue.Text + " ");
                                                }
                                            }
                                        }
                                        text.AppendLine();
                                    }
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
            // If XLSX processing fails, create empty content
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
