
using NUnit.Framework;
using SearchEngine.services;
using System.Collections.Generic;
using System.Linq;

namespace SearchEngine.Tests.Indexing
{
    public class IndexerTests
    {
        [Test]
        public void Empty_Document_Is_Handled()
        {
            var index = new InvertedIndexService();
            var id = index.IndexDocument("doc://empty", new Dictionary<string,long>());
            var results = index.SearchDocuments(new List<string>{"anything"});
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void New_Document_Becomes_Searchable()
        {
            var index = new InvertedIndexService();
            index.IndexDocument("doc://1", new Dictionary<string,long>{{"api",3}});
            var results = index.SearchDocuments(new List<string>{"api"});
            Assert.That(results, Is.Not.Empty);
        }

        [Test, Ignore("Removal not implemented in InvertedIndexService")]
        public void Removing_Document_Removes_From_Results()
        {
            Assert.Ignore("Remove operation is not implemented in this repo's InvertedIndexService.");
        }
    }
}
