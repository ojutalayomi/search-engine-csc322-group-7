
using NUnit.Framework;
using SearchEngine.services;
using SearchEngine_.utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchEngine.Tests.Matching
{
    public class MatchingTests
    {
        [Test]
        public void Single_Word_Query_Returns_Matches()
        {
            var index = new InvertedIndexService();
            index.IndexDocument("A", new Dictionary<string,long>{{"engine",2},{"search",1}});
            index.IndexDocument("B", new Dictionary<string,long>{{"engine",1}});
            var results = index.SearchDocuments(new List<string>{"engine"});
            Assert.That(results.Count, Is.GreaterThanOrEqualTo(2));
        }

        [Test]
        public void Only_Stopwords_Query_Returns_Empty()
        {
            var index = new InvertedIndexService();
            var filter = new StopWordFilter();
            var tokenizer = new QueryTokenizer(filter);
            var tokens = tokenizer.TokenizeQuery("a the to");
            var results = index.SearchDocuments(tokens.Select(t=>t.Word).ToList());
            Assert.That(results, Is.Empty);
        }
    }
}
