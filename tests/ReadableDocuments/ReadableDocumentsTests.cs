
using NUnit.Framework;
using SearchEngine_.ReadableDocuments;
using System.IO;
using System.Text;

namespace SearchEngine.Tests.Readers
{
    public class ReadableDocumentsTests
    {
        [Test]
        public void ReadableTextDocument_Reads_Words()
        {
            var data = "Hello world! This is a test.";
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(data));
            using var sr = new StreamReader(ms);
            var doc = new ReadableTextDocument(sr);
            doc.OpenDocument();
            var words = new System.Collections.Generic.List<string>();
            string? w;
            while((w = doc.ReadNextWord()) != null) words.Add(w);
            Assert.That(words, Does.Contain("hello"));
            Assert.That(words, Does.Contain("world"));
        }

        [Test]
        public void Factory_Creates_Html_And_Xml_Readers()
        {
            var factory = new ReadableDocumentFactory();
            using var msHtml = new MemoryStream(Encoding.UTF8.GetBytes("<html><body>Index engine</body></html>"));
            using var srHtml = new StreamReader(msHtml);
            var html = factory.CreateReadableDocument("text/html", srHtml);
            html.OpenDocument();
            Assert.NotNull(html.ReadNextWord());

            using var msXml = new MemoryStream(Encoding.UTF8.GetBytes("<root><x>Index engine</x></root>"));
            using var srXml = new StreamReader(msXml);
            var xml = factory.CreateReadableDocument("application/xml", srXml);
            xml.OpenDocument();
            Assert.NotNull(xml.ReadNextWord());
        }
    }
}
