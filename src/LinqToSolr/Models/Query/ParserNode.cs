using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SS.LinqToSolr.Models.Query
{
    public class ParserNode : QueryNode
    {
        private QueryParser _parser;
        public ParserNode(QueryParser parser)
        {
            _parser = parser;
        }
    }
}
