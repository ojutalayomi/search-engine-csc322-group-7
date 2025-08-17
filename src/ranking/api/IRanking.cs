﻿using SearchEngine_.indexing.models;
using SearchEngine_.ranking.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchEngine_.ranking.api
{
    public interface IRanking
    {
        List<ScoredDocumentIndex> Rank(List<DocumentIndex> scoredIndex, int[] tokenIds);

    }
}
