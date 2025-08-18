using Microsoft.AspNetCore.Mvc;
using SearchEngine_.models;
using SearchEngine_.indexing.api;
using SearchEngine_.indexing.impl;
using SearchEngine_.ReadableDocuments;
using SearchEngine_.utils;
using SearchEngine_.indexing.models;
using MySql.Data.MySqlClient;
using SearchEngine_.services;

namespace SearchEngine_.controllers;

/// <summary>
/// Web API controller for search engine functionality.
/// Provides endpoints for searching, indexing, and auto-complete.
/// </summary>
[ApiController]
[Route("api/")]
public class SearchController : ControllerBase
{
    private readonly IInvertedIndexStorage _storage;
    private readonly SearchEngineService _searchEngine;
    private readonly InvertedIndexService _inMemoryIndex;
    private readonly IReadableDocumentFactory _documentFactory;
    private readonly IStopWordFilter _stopWordFilter;
    private readonly IQueryTokenizer _queryTokenizer;
    
    public SearchController(InvertedIndexStorageFactory indexService, SearchEngineService searchEngine, InvertedIndexService inMemoryIndex, IReadableDocumentFactory documentFactory, IStopWordFilter stopWordFilter, IQueryTokenizer queryTokenizer)
    {
        _storage = indexService.Create();
        _searchEngine = searchEngine;
        _inMemoryIndex = inMemoryIndex;
        _documentFactory = documentFactory;
        _stopWordFilter = stopWordFilter;
        _queryTokenizer = queryTokenizer;
    }
    
    /// <summary>
    /// Search for documents based on a query.
    /// Response time target: &lt; 10ms.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="maxResults">Maximum number of results (default: 100)</param>
    /// <returns>Ranked search results</returns>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int maxResults = 50)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query cannot be empty");
        
        try
        {
            var startTime = DateTime.UtcNow;

            // Tokenize query
            var tokensFromQuery = _queryTokenizer.TokenizeQuery(query);
            var words = tokensFromQuery.Select(t => t.Word).ToList();

            // Map words to DB token ids
            var dbTokens = MapWordsToDbTokens(words);
            if (dbTokens.Length == 0)
            {
                return Ok(new
                {
                    Query = query,
                    Results = Array.Empty<object>(),
                    TotalResults = 0,
                    ResponseTimeMs = (DateTime.UtcNow - startTime).TotalMilliseconds,
                    PerformanceTarget = "Target: <10ms",
                    Timestamp = DateTime.UtcNow
                });
            }

            // Match and rank
            var matcher = new SearchEngine_.matching.Matcher(_storage);
            var ranked = matcher.MatchToken(dbTokens);

            List<object> limitedResults;
            int totalResults;
            if (ranked.Length == 0)
            {
                // Fallback to in-memory search if DB has no matches
                var fallback = await _searchEngine.SearchAsync(query);
                totalResults = fallback.Count;
                limitedResults = fallback
                    .Take(maxResults)
                    .Select(r => new
                    {
                        documentLink = r.DocumentLink,
                        documentName = r.DocumentName,
                        relevanceScore = r.RelevanceScore,
                        snippet = r.Snippet,
                        documentId = r.DocumentId,
                        indexedAt = r.IndexedAt
                    })
                    .Cast<object>()
                    .ToList();
            }
            else
            {
                totalResults = ranked.Length;
                limitedResults = ranked
                    .Take(maxResults)
                    .Select(r => new
                    {
                        documentLink = r.DocumentLink,
                        documentName = r.DocumentLink,
                        relevanceScore = (r is SearchEngine_.ranking.models.ScoredDocumentIndex s) ? s.Score : 0,
                        snippet = "Document containing relevant content",
                        documentId = r.Id,
                        indexedAt = DateTime.UtcNow
                    })
                    .Cast<object>()
                    .ToList();
            }

            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            return Ok(new
            {
                Query = query,
                Results = limitedResults,
                TotalResults = totalResults,
                ResponseTimeMs = responseTime,
                PerformanceTarget = "Target: <10ms",
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Search failed", Message = ex.Message });
        }
    }
       
    /// <summary>
    /// Submit a document for indexing.
    /// </summary>
    /// <param name="request">Document indexing request</param>
    /// <returns>Indexing result</returns>
    [HttpPost("index")]
    public async Task<IActionResult> IndexDocument([FromBody] QueueRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.DocumentURL))
            return BadRequest("Invalid request");
        
        try
        {
            // Process the document
            var index = await _searchEngine.ProcessQueueRequestAsync(request);
            
            // Persist index via storage implementation
            _storage.StoreIndex(index);
            
            // Simulate async operation
            await Task.Delay(1);
            
            return Ok(new
            {
                Message = "Document indexed successfully",
                DocumentId = index.Id,
                DocumentUrl = request.DocumentURL,
                WordCount = index.FrequencyDict.Count, // index.FrequencyDict.Count
                IndexedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Indexing failed", Message = ex.Message });
        }
    }
    
    /// <summary>
    /// Upload and index a file.
    /// </summary>
    /// <param name="file">The file to upload and index</param>
    /// <returns>Indexing result</returns>
    [HttpPost("index/file")]
    public async Task<IActionResult> IndexFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided");
        
        try
        {
            var id = generateId();
            var contentType = GetMimeType(file);

            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);

            var document = _documentFactory.CreateReadableDocument(contentType, reader);
            document.OpenDocument();

            var words = new List<string>();
            string? word;
            while ((word = document.ReadNextWord()) != null)
            {
                words.Add(word);
            }

            var filteredWords = _stopWordFilter.FilterOutStopWords(words);
            var frequencyDict = filteredWords
                .GroupBy(w => w)
                .ToDictionary(g => g.Key, g => (long)g.Count());

            var index = new DocumentIndex
            {
                Id = id,
                DocumentLink = $"upload:{file.FileName}",
                DocumentType = contentType,
                FrequencyDict = frequencyDict,
                totalTermCount = words.Count
            };

            _storage.StoreIndex(index);

            await Task.Delay(1);

            return Ok(new
            {
                Message = "File indexed successfully",
                DocumentId = index.Id,
                DocumentName = file.FileName,
                FileSize = file.Length,
                IndexedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "File indexing failed", Message = ex.Message });
        }
    }
    
    private string generateId()
    {
        return "req_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "_" + Guid.NewGuid().ToString("N").Substring(0, 8);
    }

    private static string GetMimeType(IFormFile file)
    {
        var contentType = file.ContentType;
        if (!string.IsNullOrWhiteSpace(contentType))
        {
            return contentType;
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            ".html" => "text/html",
            ".htm" => "text/html",
            ".xml" => "application/xml",
            _ => "text/plain"
        };
    }
    
    /// <summary>
    /// Get auto-complete suggestions for a partial word.
    /// </summary>
    /// <param name="partialWord">Partial word to complete</param>
    /// <param name="maxSuggestions">Maximum number of suggestions (default: 10)</param>
    /// <returns>List of word suggestions</returns>
    [HttpGet("search/autocomplete")]
    public IActionResult GetAutoComplete([FromQuery] string partialWord, [FromQuery] int maxSuggestions = 10)
    {
        if (string.IsNullOrWhiteSpace(partialWord))
            return BadRequest("Partial word cannot be empty");
        
        try
        {
            var suggestions = GetDbAutoComplete(partialWord, maxSuggestions);
            
            return Ok(new
            {
                PartialWord = partialWord,
                Suggestions = suggestions,
                Count = suggestions.Count,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Auto-complete failed", Message = ex.Message });
        }
    }

    private SearchEngine_.indexing.models.Token[] MapWordsToDbTokens(List<string> words)
    {
        var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString) || words.Count == 0)
            return Array.Empty<SearchEngine_.indexing.models.Token>();

        try
        {
            var distinctWords = words.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var tokens = new List<SearchEngine_.indexing.models.Token>();

            using var connection = new MySqlConnection(connectionString);
            connection.Open();

            var paramNames = new List<string>();
            using var cmd = connection.CreateCommand();
            for (int i = 0; i < distinctWords.Count; i++)
            {
                var p = "@v" + i;
                paramNames.Add(p);
                cmd.Parameters.AddWithValue(p, distinctWords[i]);
            }
            cmd.CommandText = $"SELECT id, value FROM inverted_index_table WHERE value IN ({string.Join(",", paramNames)})";

            var found = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32("id");
                    var value = reader.GetString("value");
                    found[value] = id;
                }
            }

            foreach (var w in distinctWords)
            {
                if (found.TryGetValue(w, out var id))
                {
                    tokens.Add(new SearchEngine_.indexing.models.Token
                    {
                        Id = id.ToString(),
                        Value = w
                    });
                }
            }

            return tokens.ToArray();
        }
        catch
        {
            return Array.Empty<SearchEngine_.indexing.models.Token>();
        }
    }

    private List<string> GetDbAutoComplete(string partialWord, int maxSuggestions)
    {
        var connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
            return _inMemoryIndex.GetAutoCompleteSuggestions(partialWord, maxSuggestions);

        try
        {
            using var connection = new MySqlConnection(connectionString);
            connection.Open();
            using var cmd = new MySqlCommand("SELECT value FROM inverted_index_table WHERE value LIKE @p ORDER BY LENGTH(value) ASC, value ASC LIMIT @lim", connection);
            cmd.Parameters.AddWithValue("@p", partialWord + "%");
            cmd.Parameters.AddWithValue("@lim", maxSuggestions);
            var list = new List<string>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(reader.GetString("value"));
            }
            if (list.Count == 0)
                return _inMemoryIndex.GetAutoCompleteSuggestions(partialWord, maxSuggestions);
            return list;
        }
        catch
        {
            return _inMemoryIndex.GetAutoCompleteSuggestions(partialWord, maxSuggestions);
        }
    }

    /// <summary>
    /// Search statistics for UI.
    /// </summary>
    [HttpGet("search/statistics")]
    public IActionResult GetStatistics()
    {
        try
        {
            var stats = _inMemoryIndex.GetStatistics();
            return Ok(new { statistics = stats });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Statistics retrieval failed", Message = ex.Message });
        }
    }
    
    /// <summary>
    /// Health check endpoint.
    /// </summary>
    /// <returns>Service health status</returns>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "Search Engine API",
            Version = "1.0.0",
            Timestamp = DateTime.UtcNow,
            PerformanceTargets = new
            {
                QueryResponseTime = "< 10ms",
                IndexingDelay = "< 2 hours"
            }
        });
    }
}
