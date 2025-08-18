namespace SearchEngine_.models;

/// <summary>
/// Represents a tokenized word with its database index.
/// </summary>
public class Token
{
    /// <summary>
    /// Gets or sets the unique identifier for the token.
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Gets or sets the word text.
    /// </summary>
    public string Word { get; set; } = string.Empty;
}
