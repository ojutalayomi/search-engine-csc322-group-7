using SearchEngine_.indexing.api;
using SearchEngine_.indexing.models;
using SearchEngine_.ranking.api;
using SearchEngine_.ranking.models;

namespace SearchEngine_.ranking.impl
{
    public class TFIDFRankingAlgorithm:IRanking
    {
        private readonly IInvertedIndexStorage _invertedIndexStorage;
        private long _totalCorpusSize;

        public TFIDFRankingAlgorithm(IInvertedIndexStorage invertedIndexStorage)
        {
            _invertedIndexStorage = invertedIndexStorage;
            _totalCorpusSize = _invertedIndexStorage.GetTotalCorpusSize();
        }

        public List<ScoredDocumentIndex> Rank(List<DocumentIndex> indexes, Token[] tokens)
        {
            if (tokens == null || tokens.Length == 0)
            {
                return indexes.Select(d => new ScoredDocumentIndex(d, 0)).ToList();
            }

            int[] tokenIds = tokens.Select(t => int.Parse(t.Id)).ToArray();

            // 1. Get posting list sizes for the relevant tokens
            Dictionary<int, long> postingListSizes = _invertedIndexStorage.GetPostingListSizes(tokenIds);
            _totalCorpusSize = _invertedIndexStorage.GetTotalCorpusSize();

            // 2. Calculate scores for each document
            var scoredDocuments = new List<ScoredDocumentIndex>();

            foreach (var docIndex in indexes)
            {
                double documentScore = 0;

                foreach (var token in tokens)
                {
                    var tokenId = int.Parse(token.Id);
                    var term = token.Value; // frequency keyed by term string

                    if (docIndex.FrequencyDict.TryGetValue(term, out long termFrequency))
                    {
                        // Calculate TF: (term count in doc) / (total terms in doc)
                        double tf = (double)termFrequency / docIndex.totalTermCount;

                        // Calculate IDF: log(total docs / docs with term)
                        double idf = 0;
                        if (postingListSizes.ContainsKey(tokenId) && postingListSizes[tokenId] > 0)
                        {
                            idf = Math.Log((double)_totalCorpusSize / postingListSizes[tokenId]);
                        }

                        // Calculate TF-IDF score for the term in the document
                        documentScore += tf * idf;
                    }
                }

                // Add the document and its final score to the list
                scoredDocuments.Add(new ScoredDocumentIndex(docIndex, documentScore));
            }

            // 3. Rank documents by score in descending order
            return scoredDocuments.OrderByDescending(d => d.Score).ToList();
        }
    }
    
}
