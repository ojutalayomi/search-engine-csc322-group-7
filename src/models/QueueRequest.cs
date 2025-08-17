namespace SearchEngine_.models;

/// <summary>
/// Represents a request to be processed by the search engine queue.
/// </summary>
public class QueueRequest
{
    /// <summary>
    /// Gets or sets the unique identifier for the request.
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the document.
    /// </summary>
    public string DocumentName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the URL of the document.
    /// </summary>
    public string DocumentURL { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the date and time when the request was submitted.
    /// </summary>
    public DateTime DateTimeSubmitted { get; set; }
    
    /// <summary>
    /// Gets or sets the priority level (0-9). Priority 0 means index immediately.
    /// </summary>
    public int Priority { get; set; }
}
