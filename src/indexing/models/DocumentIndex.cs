namespace SearchEngine_.indexing.models
{
    public class DocumentIndex
    {
        public string Id { get; set; }
        public string DocumentLink { get; set; }
        public string DocumentType { get; set; }
        public IDictionary<string, long> FrequencyDict = new Dictionary<string, long>();
        public long totalTermCount {get; set; }
    }
}
