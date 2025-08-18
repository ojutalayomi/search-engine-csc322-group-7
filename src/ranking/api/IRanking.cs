using SearchEngine_.indexing.models;
using SearchEngine_.ranking.models;

namespace SearchEngine_.ranking.api
{
    public interface IRanking
    {
        List<ScoredDocumentIndex> Rank(List<DocumentIndex> scoredIndex, Token[] tokens);

    }
}
