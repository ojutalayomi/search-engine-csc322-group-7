namespace SearchEngine.interfaces;

/// <summary>
/// Interface for filtering out stop words from a list of words.
/// </summary>
public interface IStopWordFilter
{
    /// <summary>
    /// Filters out stop words from the provided list of words.
    /// </summary>
    /// <param name="words">The list of words to filter.</param>
    /// <returns>A filtered list with stop words removed.</returns>
    List<string> FilterOutStopWords(List<string> words);
}
