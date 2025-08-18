using NUnit.Framework;
using SearchEngine_.utils;
using System.Linq;

namespace SearchEngine_.tests.Utils
{
    public class QueryTokenizerTests
    {
        [Test]
        public void Tokenize_Removes_Stopwords_And_Lowers()
        {
            var filter = new StopWordFilter();
            var tokenizer = new QueryTokenizer(filter);
            var tokens = tokenizer.TokenizeQuery("The University of Lagos");
            Assert.That(tokens.Select(t=>t.Word), Is.EquivalentTo(new []{"university","lagos"}));
        }

        [Test]
        public void Tokenize_Empty_Query_Returns_Empty_List()
        {
            var filter = new StopWordFilter();
            var tokenizer = new QueryTokenizer(filter);
            var tokens = tokenizer.TokenizeQuery("");
            Assert.That(tokens, Is.Empty);
        }
    }
}
