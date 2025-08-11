using SearchEngine.interfaces;
using SearchEngine.models;

/// <summary>
/// Interface for tokenizing queries, removing stop words, and searching the database for word indices.
/// </summary>
public interface IQueryTokenizer
{
    /// <summary>
    /// Tokenizes a query, removes stop words, and searches the database for word indices.
    /// </summary>
    /// <param name="query">The search query to tokenize.</param>
    /// <returns>A list of tokens with their database indices.</returns>
    List<Token> TokenizeQuery(string query);
}
