using SearchEngine_.indexing.api;
<<<<<<< HEAD
using SearchEngine_.ranking.impl;
using SearchEngine_.indexing.models;

namespace SearchEngine_.matching
{
    public class Matcher: IMatcher
=======
using SearchEngine_.indexing.models;
using SearchEngine_.ranking.impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine_.matching
{
    public class Matcher:IMatcher
>>>>>>> 2e5dba8f14fa4cbcb1ed9d7bdd65d7befb3ae391
    {
        public Matcher(IInvertedIndexStorage invertedIndexStorage)
        {
            InvertedIndexStorage = invertedIndexStorage;
            RankingAlgorithm = new TFIDFRankingAlgorithm(InvertedIndexStorage);
        }

        public IInvertedIndexStorage InvertedIndexStorage { get; }
        public TFIDFRankingAlgorithm RankingAlgorithm { get; }

        public DocumentIndex[] MatchToken(Token[] token)
        {
            // This method should match the tokens against the inverted index storage
            // And then rank them using TFIDF.

            int[] tokenIds = token.Select(t => int.Parse(t.Id)).ToArray();
            List<DocumentIndex> matchedDocuments = InvertedIndexStorage.MatchTokens(token);
            if (matchedDocuments == null || matchedDocuments.Count == 0)
            {
                return Array.Empty<DocumentIndex>();
            }

            return RankingAlgorithm.Rank(matchedDocuments, tokenIds).ToArray();
        }
    }
}
