namespace SearchEngine_.models;

/// <summary>
/// Represents an indexed document with word frequencies.
/// </summary>
public class Index
{
    /// <summary>
    /// Gets or sets the unique identifier for the index.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the document link/URL.
    /// </summary>
    public string DocumentLink { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the frequency dictionary showing word frequencies.
    /// </summary>
    public Dictionary<string, int> FrequencyDict { get; set; } = new Dictionary<string, int>();
}
