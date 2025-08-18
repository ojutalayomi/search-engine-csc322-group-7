using NUnit.Framework;
using SearchEngine_.utils;
using System.Collections.Generic;

namespace SearchEngine_.tests.Utils
{
    public class StopWordFilterTests
    {
        [Test]
        public void Filters_Common_Stopwords()
        {
            var filter = new StopWordFilter();
            var input = new List<string>{ "the","university","of","lagos"};
            var output = filter.FilterOutStopWords(input);
            Assert.That(output, Is.EquivalentTo(new []{"university","lagos"}));
        }
    }
}
