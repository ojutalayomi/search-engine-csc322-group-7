
using NUnit.Framework;
using SearchEngine.services;
using System.Collections.Generic;
using System.Linq;

namespace SearchEngine.Tests.Ranking
{
    public class RankingTests
    {
        [Test]
        public void Higher_Frequency_Ranks_Higher()
        {
            var index = new InvertedIndexService();
            var id1 = index.IndexDocument("doc://high", new Dictionary<string,long>{{"engine",10}});
            var id2 = index.IndexDocument("doc://low", new Dictionary<string,long>{{"engine",1}});
            var results = index.SearchDocuments(new List<string>{"engine"});
            Assert.That(results.First().DocumentId, Is.EqualTo(id1));
        }
    }
}
