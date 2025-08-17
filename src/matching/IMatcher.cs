using SearchEngine_.indexing.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine_.matching;

interface IMatcher
{
    DocumentIndex[] MatchToken(Token[] token);
}
