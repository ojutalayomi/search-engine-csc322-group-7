using Microsoft.AspNetCore.Mvc;
using SearchEngine_.models;
// using SearchEngine.services;

namespace SearchEngine_.controllers;

/// <summary>
/// Web API controller for search engine functionality.
/// Provides endpoints for searching, indexing, and auto-complete.
/// </summary>
[ApiController]
[Route("api/")]
public class SearchController : ControllerBase
{
    // private readonly InvertedIndexService _indexService;
    // private readonly SearchEngineService _searchEngine;
    
    /// <summary>
    /// Search for documents based on a query.
    /// Response time target: &lt; 10ms.
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="maxResults">Maximum number of results (default: 100)</param>
    /// <returns>Ranked search results</returns>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int maxResults = 100)
    {
        if (string.IsNullOrWhiteSpace(query))
            return BadRequest("Query cannot be empty");
        
        try
        {
            var startTime = DateTime.UtcNow;
            
            // Tokenize and search
            var results = "My name is <NAME> and I am a software engineer."; // A method that accepts the query as an argument and return the results
            
            // Limit results
            var limitedResults = results.Take(maxResults).ToList();
            
            var responseTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            
            return Ok(new
            {
                Query = query,
                Results = limitedResults,
                TotalResults = results.Length, //This should be results.Count .i.e the number of results 
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
            // var index = await _searchEngine.ProcessQueueRequestAsync(request);
            
            // Add to inverted index
            // var documentId = _indexService.IndexDocument(request.DocumentURL, index.FrequencyDict);
            
            return Ok(new
            {
                Message = "Document indexed successfully",
                DocumentId = "dfdgfghf", // documentId
                DocumentUrl = request.DocumentURL,
                WordCount = 4, // index.FrequencyDict.Count
                IndexedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "Indexing failed", Message = ex.Message });
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
