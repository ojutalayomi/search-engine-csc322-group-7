# University Search Engine Project

## Overview:
This project is a Search Engine API built as part of CSC322 coursework. The engine is designed to index and search documents uploaded. It implements indexing, query parsing, matching, and ranking algorithms with a simple web interface for interaction.

The system is modular, implemented as an API, and can be integrated into the University’s website or intranet to allow efficient information retrieval.

### Problem Statement:
The University required a search tool for its website and intranet to:
- Accept multiple document types (pdf, doc, docx, ppt, pptx, xls, xlsx, txt, html, xml).
- Provide query response time around 0.01s.
- Support query auto-complete.
- Index new/updated documents within 2 hours.

### Solution Designed:
Our solution is a modular search engine composed of:
- Document Representation (Indexing)
- Query Representation (Parsing)
- Matching Function (Retrieval)
- Ranking Algorithm (TF-IDF)
- Results Display (Web UI + API)

## System Architecture:
The search engine consists of the following modules:
- Document Readers: Handle various file formats (PDF, TXT, HTML, XML).
- Preprocessing Utilities: Tokenizer, Stop-word Filter, Link Resolver.
- Indexing Engine: Inverted Index stored in a database (MySQL).
- Matching Engine: Retrieves candidate documents based on tokens.
- Ranking Engine: Applies TF-IDF ranking to order results.
- API Layer: Provides endpoints for search requests.
- Web Interface: A minimal search page for user interaction.

### Project Structure:
```text
search-engine-csc322-group-7-ayo/
│
├── Program.cs                       // Application entrypoint (bootstraps API host)
├── SearchEngine_.csproj             // Main project file
├── appsettings.json                  // Application configuration (default)
├── appsettings.Development.json      // Development-specific settings
│
├── src/                              // Core source code
│   ├── controllers/
│   │   └── SearchController.cs       // API endpoints for search and indexing
│   │
│   ├── helpers/
│   │   └── StorageHelper.cs          // Utility for managing file/document storage
│   │
│   ├── indexing/                     // Index building & storage logic
│   │   ├── api/
│   │   │   ├── IIndexer.cs           // Interface for indexer implementations
│   │   │   └── IInvertedIndexStorage.cs // Interface for persistent index storage
│   │   ├── impl/
│   │   │   ├── Indexer.cs            // Core indexer implementation
│   │   │   └── MySqlBasedInvertedTokenStorage.cs // MySQL-backed index persistence
│   │   └── models/
│   │       └── DocumentIndex.cs      // Data model for indexed document entries
│   │
│   ├── matching/
│   │   └── Matcher.cs                // Query–document matching logic (uses ranking)
│   │
│   ├── models/
│   │   ├── Index.cs                  // Index data structure representation
│   │   ├── QueueRequest.cs           // Queue request model for async ops
│   │   └── Token.cs                  // Represents a tokenized word in the index
│   │
│   ├── ranking/
│   │   ├── api/                      // Interfaces for ranking algorithms
│   │   ├── impl/                     // Placeholder for ranking algorithm classes
│   │   └── models/                   // Ranking-related models
│   │
│   ├── services/
│   │   ├── InvertedIndexService.cs   // Service for adding, searching, ranking docs
│   │   └── SearchEngineService.cs    // High-level orchestration of search workflow
│   │
│   ├── utils/
│   │   ├── IStopWordFilter.cs        // Interface for stopword filtering
│   │   ├── IQueryTokenizer.cs        // Interface for query tokenization
│   │   ├── ILinkResolver.cs          // Interface for resolving document links
│   │   ├── StopWordFilter.cs         // Implementation: removes stopwords
│   │   ├── QueryTokenizer.cs         // Implementation: splits queries into tokens
│   │   └── LinkResolver.cs           // Implementation: fetches docs from URLs
│   │
│   └── ReadableDocuments/            // Document readers by file type
│       ├── ReadablePdfDocument.cs    // Reads and extracts text from PDF files
│       ├── ReadableDocxDocument.cs   // Reads and extracts text from DOCX files
│       ├── ReadablePptDocument.cs    // Reads and extracts text from PPT files
│       ├── ReadablePptxDocument.cs   // Reads and extracts text from PPTX files
│       ├── ReadableXlsDocument.cs    // Reads and extracts text from XLS files
│       ├── ReadableXlsxDocument.cs   // Reads and extracts text from XLSX files
│       ├── ReadableTextDocument.cs   // Reads and extracts text from TXT files
│       ├── ReadableHtmlDocument.cs   // Reads and extracts text from HTML files
│       ├── ReadableXmlDocument.cs    // Reads and extracts text from XML files
│       └── ReadableDocumentFactory.cs// Factory to create readers by file type
│
└── tests/                            // NUnit test project
    ├── SearchEngine.Tests.csproj     // Test project file
    ├── TestGuards.cs                 // Helper to skip tests if types are missing
    ├── TestData/                     // Sample documents for test inputs
    │   ├── empty.txt                 // Blank file for empty doc test
    │   ├── stopwords.txt             // File containing only stopwords
    │   ├── freq.txt                  // File with repeated “API” for frequency test
    │   ├── sample.txt                // General sample text file
    │   ├── sample.html               // Sample HTML file
    │   └── sample.xml                // Sample XML file
    │
    ├── Controllers/
    │   └── SearchControllerTests.cs  // API endpoint tests
    ├── Indexing/
    │   └── IndexerTests.cs           // Indexing tests (file types, stopwords, freq)
    ├── Matching/
    │   └── MatcherTests.cs           // Query & matching tests
    ├── Ranking/
    │   └── RankingTests.cs           // Ranking and relevance tests
    ├── ReadableDocuments/
    │   └── ReadableDocumentsTests.cs // Tests for document readers
    ├── Services/
    │   └── ServicesTests.cs          // Auto-complete & performance tests
    └── Utils/
        ├── StopWordFilterTests.cs    // Tests for stopword filtering
        └── QueryTokenizerTests.cs    // Tests for query tokenization

```



### Class Diagram:

#### 1. Document Readers and Utilities
``` mermaid
classDiagram
direction TB

class IReadableDocument {
  <<interface>>
  +string ReadContent()
}

class ReadableDocumentFactory {
  -Dictionary<string, Func<IReadableDocument>> _factories
  +IReadableDocument Create(string path)
}

class ReadablePdfDocument {
  -string _filePath
  +string ReadContent()
}
class ReadableDocxDocument {
  -string _filePath
  +string ReadContent()
}
class ReadablePptDocument {
  -string _filePath
  +string ReadContent()
}
class ReadablePptxDocument {
  -string _filePath
  +string ReadContent()
}
class ReadableXlsDocument {
  -string _filePath
  +string ReadContent()
}
class ReadableXlsxDocument {
  -string _filePath
  +string ReadContent()
}
class ReadableTextDocument {
  -string _filePath
  +string ReadContent()
}
class ReadableHtmlDocument {
  -string _filePath
  +string ReadContent()
}
class ReadableXmlDocument {
  -string _filePath
  +string ReadContent()
}

IReadableDocument <|.. ReadablePdfDocument
IReadableDocument <|.. ReadableDocxDocument
IReadableDocument <|.. ReadablePptDocument
IReadableDocument <|.. ReadablePptxDocument
IReadableDocument <|.. ReadableXlsDocument
IReadableDocument <|.. ReadableXlsxDocument
IReadableDocument <|.. ReadableTextDocument
IReadableDocument <|.. ReadableHtmlDocument
IReadableDocument <|.. ReadableXmlDocument

ReadableDocumentFactory --> IReadableDocument

class IStopWordFilter {
  <<interface>>
  +IEnumerable<string> FilterOutStopWords(IEnumerable<string>)
}
class StopWordFilter {
  -HashSet<string> _stopWords
  +IEnumerable<string> FilterOutStopWords(IEnumerable<string>)
}

class IQueryTokenizer {
  <<interface>>
  +IEnumerable<Token> Tokenize(string)
}
class QueryTokenizer {
  -IStopWordFilter _filter
  +IEnumerable<Token> Tokenize(string)
}

class ILinkResolver {
  <<interface>>
  +Task<string> ResolveAsync(string link)
}
class LinkResolver {
  -HttpClient _client
  +Task<string> ResolveAsync(string link)
}

IStopWordFilter <|.. StopWordFilter
IQueryTokenizer <|.. QueryTokenizer
ILinkResolver <|.. LinkResolver

```
All document readers (PDF, DOCX, PPT, HTML, etc.) implement a common IReadableDocument interface, which ensures they can extract text in a unified way. The ReadableDocumentFactory decides which reader to instantiate based on file type.
Utility classes (StopWordFilter, QueryTokenizer, LinkResolver) provide reusable services for cleaning, tokenizing, and fetching documents.



### 2. Indexing, Matching and Ranking
``` mermaid
classDiagram
direction TB

class DocumentIndex {
  +string Id
  +string DocumentLink
  +string DocumentType
  +Dictionary<string,long> FrequencyDict
  +long TotalTermCount
}

class IIndexer {
  <<interface>>
  +void AddOrUpdateDocument()
  +void RemoveDocument()
}
class Indexer {
  -IInvertedIndexStorage _storage
  +void AddOrUpdateDocument()
  +void RemoveDocument()
  +IEnumerable<DocumentIndex> Search(string term)
}

class IInvertedIndexStorage {
  <<interface>>
  +void SaveToken(string token, DocumentIndex doc)
  +IEnumerable<DocumentIndex> GetDocuments(string token)
}
class MySqlBasedInvertedTokenStorage {
  -MySqlConnection _connection
  +void SaveToken(string token, DocumentIndex doc)
  +IEnumerable<DocumentIndex> GetDocuments(string token)
}

IIndexer <|.. Indexer
IInvertedIndexStorage <|.. MySqlBasedInvertedTokenStorage
Indexer --> IInvertedIndexStorage
Indexer --> DocumentIndex

class IMatcher {
  <<interface>>
  +IEnumerable<DocumentIndex> Match(string query)
}
class Matcher {
  -IRankingAlgorithm _ranking
  +IEnumerable<DocumentIndex> Match(string query)
}
IMatcher <|.. Matcher

class IRankingAlgorithm {
  <<interface>>
  +IEnumerable<DocumentIndex> Rank(IEnumerable<DocumentIndex>)
}
class TFIDFRankingAlgorithm {
  +IEnumerable<DocumentIndex> Rank(IEnumerable<DocumentIndex>)
}
IRankingAlgorithm <|.. TFIDFRankingAlgorithm

Matcher --> IRankingAlgorithm
```
The Indexer builds an inverted index of documents and relies on a storage backend (MySqlBasedInvertedTokenStorage) to persist results. Each document’s tokens are represented as DocumentIndex objects.
The Matcher handles query–document matching and delegates result ordering to a ranking algorithm (e.g., TFIDFRankingAlgorithm).


#### 3. Controller Layer
``` mermaid
classDiagram
direction TB

class SearchController {
  -SearchEngineService _service
  +Task<IActionResult> Search(string query)
  +Task<IActionResult> IndexDocument(IFormFile file)
}

class SearchEngineService {
  -InvertedIndexService _indexService
  -IStopWordFilter _stopWordFilter
  -IQueryTokenizer _tokenizer
  -ILinkResolver _linkResolver
  +Task<IEnumerable<SearchResult>> SearchAsync(string query)
  +Task IndexDocumentAsync(string path)
}

class InvertedIndexService {
  -Dictionary<string,List<DocumentIndex>> _invertedIndex
  +void AddOrUpdateDocument(int id, string name, string content)
  +IEnumerable<DocumentIndex> Query(string term)
  +void RemoveDocument(int id)
}

SearchController --> SearchEngineService
SearchEngineService --> InvertedIndexService
```
The SearchController provides API endpoints (/search, /index). It delegates to SearchEngineService, which orchestrates tokenization, indexing, and ranking through InvertedIndexService.


### Sequence Diagrams
#### Search Flow
``` mermaid
sequenceDiagram
    participant U as User
    participant SC as SearchController
    participant QT as QueryTokenizer
    participant IS as InvertedIndexStorage
    participant M as Matcher
    participant R as TFIDFRankingAlgorithm

    U->>SC: Search(query, maxResults)
    SC->>QT: TokenizeQuery(query)
    QT-->>SC: List<Token>
    SC->>M: MatchToken(tokens)
    M->>IS: Retrieve posting lists
    IS-->>M: List<DocumentIndex>
    M-->>SC: Candidate documents
    SC->>R: Rank(candidates, tokenIds)
    R-->>SC: List<ScoredDocumentIndex>
    SC-->>U: Top N Results (ranked documents)
```
#### Indexing Flow
``` mermaid
sequenceDiagram
    participant U as User
    participant SC as SearchController
    participant LR as LinkResolver
    participant DF as ReadableDocumentFactory
    participant RD as IReadableDocument
    participant SW as StopWordFilter
    participant IS as InvertedIndexStorage

    U->>SC: IndexDocument(QueueRequest)
    SC->>LR: ResolveLink(documentURL)
    LR-->>SC: StreamReader
    SC->>DF: CreateReadableDocument(mimeType, reader)
    DF-->>SC: IReadableDocument
    SC->>RD: OpenDocument()
    loop For each word
        RD-->>SC: word
        SC->>SW: FilterOutStopWords(word)
        SW-->>SC: cleanedWord
    end
    SC->>IS: StoreIndex(DocumentIndex)
    IS-->>SC: Ack
    SC-->>U: Document successfully indexed

```
### Implementation Details
#### 1. Indexing
    Each document is parsed using the appropriate ReadableDocument class.
    Text is tokenized into words using QueryTokenizer.
    Stop words are filtered using StopWordFilter.
    Remaining tokens are stored in an inverted index (MySQL-based).

#### 2. Query Processing
    Queries are tokenized and filtered similarly to documents.
    The processed query is sent to the Matcher.

#### 3. Matching
    The Matcher checks the inverted index for documents containing the query tokens.
    Candidate documents are retrieved.

#### 4. Ranking
    Retrieved documents are ranked using TF-IDF in TFIDFRankingAlgorithm.
    Documents are scored based on relevance to query terms.

#### 5. Results Display
    Results are returned via the SearchController API.
    index.html provides a simple UI with a search bar and results page.


