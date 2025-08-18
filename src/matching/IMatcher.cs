using SearchEngine_.indexing.models;
<<<<<<< HEAD
=======
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
>>>>>>> 2e5dba8f14fa4cbcb1ed9d7bdd65d7befb3ae391

namespace SearchEngine_.matching;

interface IMatcher
{
    DocumentIndex[] MatchToken(Token[] token);
}
