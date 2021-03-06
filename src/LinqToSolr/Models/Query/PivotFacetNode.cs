﻿using System.Collections.Generic;

namespace SS.LinqToSolr.Models.Query
{
    public class PivotFacetNode : IQueryNode
    {
        public PivotFacetNode(List<MethodNode> facets)
        {
            Facets = facets;
        }

        public List<MethodNode> Facets { get; private set; }
    }
}
