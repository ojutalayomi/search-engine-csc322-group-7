
using NUnit.Framework;
using SearchEngine.services;
using SearchEngine_.utils;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace SearchEngine.Tests.Services
{
    public class InvertedIndexServiceTests
    {
        [Test]
        public void Index_And_Search_Text_Document()
        {
            var index = new InvertedIndexService();
            var content = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory,"TestData","sample.txt"));
            var words = content.Split(new[]{' ','\n','\r','\t','.','!','?',';',',',':','-','\"','\''}, StringSplitOptions.RemoveEmptyEntries)
                               .Select(w=>w.ToLowerInvariant())
                               .ToList();
            var freq = words.GroupBy(w=>w).ToDictionary(g=>g.Key, g=>(long)g.Count());
            var docId = index.IndexDocument("file://sample.txt", freq);
            var results = index.SearchDocuments(new List<string>{ "engine" });
            Assert.That(results, Is.Not.Empty);
            Assert.That(results.First().DocumentId, Is.EqualTo(docId));
        }

        [Test]
        public void AutoComplete_Suggests_Prefixes()
        {
            var index = new InvertedIndexService();
            var freq = new Dictionary<string,long>{{"index",3},{"indexer",2},{"independent",1}};
            index.IndexDocument("doc://1", freq);
            var suggestions = index.GetAutoCompleteSuggestions("inde");
            Assert.That(suggestions, Does.Contain("independent"));
            Assert.That(suggestions, Does.Not.Contain("engine"));
        }

        [Test]
        public void Nonexistent_Term_Returns_Empty()
        {
            var index = new InvertedIndexService();
            index.IndexDocument("doc://1", new Dictionary<string,long>{{"alpha",1}});
            var results = index.SearchDocuments(new List<string>{"omega"});
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void Ranking_Favors_Higher_Frequency()
        {
            var index = new InvertedIndexService();
            var idA = index.IndexDocument("A", new Dictionary<string,long>{{"engine",5}});
            var idB = index.IndexDocument("B", new Dictionary<string,long>{{"engine",1}});
            var results = index.SearchDocuments(new List<string>{"engine"});
            Assert.That(results[0].DocumentId, Is.EqualTo(idA));
            Assert.That(results[1].DocumentId, Is.EqualTo(idB));
        }

        [Test]
        public void Performance_Query_Response_Time_Is_Fast()
        {
            var index = new InvertedIndexService();
            // Seed many docs
            for(int i=0;i<200;i++)
            {
                index.IndexDocument($"doc://{i}", new Dictionary<string,long>{{"engine", i%5+1},{"search",1},{"index",2}});
            }
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var results = index.SearchDocuments(new List<string>{ "engine","search"});
            sw.Stop();
            Assert.That(results, Is.Not.Empty);
            Assert.LessOrEqual(sw.ElapsedMilliseconds, 100); // be pragmatic for test env
        }
    }
}
