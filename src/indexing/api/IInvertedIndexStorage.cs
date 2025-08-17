using SearchEngine_.indexing.models;

namespace SearchEngine_.indexing.api
{
    public interface IInvertedIndexStorage
    {
        void StoreIndex(DocumentIndex index);
        //Store by opening the index and searching
        List<DocumentIndex> MatchTokens(Token[] tokens);
        long GetPostingListSize(Token token);
        //I need the total term count of index - can do
        //I need the total document count of token - can do
        Dictionary<int, long> GetPostingListSizes(int[] tokenIds);
        long GetTotalCorpusSize();
    }

}
