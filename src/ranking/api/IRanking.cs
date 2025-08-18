using SearchEngine_.indexing.models;
using SearchEngine_.ranking.models;
<<<<<<< HEAD
=======
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
>>>>>>> 2e5dba8f14fa4cbcb1ed9d7bdd65d7befb3ae391

namespace SearchEngine_.ranking.api
{
    public interface IRanking
    {
        List<ScoredDocumentIndex> Rank(List<DocumentIndex> scoredIndex, int[] tokenIds);

    }
}
