using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using SearchEngine_.models;
using SearchEngine_.ReadableDocuments;
using SearchEngine_.services;
using SearchEngine_.utils;

namespace SearchEngine_.tests.Services
{
    // Minimal fake link resolver that returns a memory stream with provided content
    class FakeLinkResolver : ILinkResolver
    {
        private readonly string _content;
        public FakeLinkResolver(string content) { _content = content; }

        public (string ContentType, StreamReader StreamReader) ResolveLink(string link, List<string> acceptableMimeTypes)
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(_content));
            return ("text/plain", new StreamReader(ms));
        }
    }

    [TestFixture]
    public class SearchEngineServiceTests
    {
        [Test]
        public async Task SearchAsync_Empty_And_StopwordOnly_Queries()
        {
            var index = new InvertedIndexService();
            var factory = new ReadableDocumentFactory();
            var filter = new StopWordFilter();
            var tokenizer = new QueryTokenizer(filter);
            var linkResolver = new FakeLinkResolver("");

            var svc = new SearchEngineService(linkResolver, factory, filter, tokenizer, index);

            var empty = await svc.SearchAsync("");
            Assert.That(empty, Is.Empty);

            var stopOnly = await svc.SearchAsync("the a an to of");
            Assert.That(stopOnly, Is.Empty);
        }

        [Test]
        public async Task ProcessQueueRequest_Indexes_And_Is_Searchable()
        {
            var text = "API design: API First. Clean API!";
            var index = new InvertedIndexService();
            var factory = new ReadableDocumentFactory();
            var filter = new StopWordFilter();
            var tokenizer = new QueryTokenizer(filter);
            var linkResolver = new FakeLinkResolver(text);
            var svc = new SearchEngineService(linkResolver, factory, filter, tokenizer, index);

            var req = new QueueRequest { Id = "1", DocumentName = "doc1", DocumentURL = "mem://doc1" };
            await svc.ProcessQueueRequestAsync(req);

            var results = await svc.SearchAsync("api");
            Assert.That(results, Is.Not.Empty);
            Assert.That(results.First().DocumentLink, Is.EqualTo("mem://doc1"));
        }
    }
}
