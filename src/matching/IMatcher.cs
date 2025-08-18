using SearchEngine_.indexing.models;

namespace SearchEngine_.matching;

interface IMatcher
{
    DocumentIndex[] MatchToken(Token[] token);
}
