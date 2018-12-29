using System;
using System.Collections.Generic;
using System.Linq;
using SS.LinqToSolr.Models;
using SS.LinqToSolr.Models.SearchResponse;
using SS.LinqToSolr.Test.Models;

namespace SS.LinqToSolr.Test
{
    public class FakeSearchContext : ISolrService
    {
        public string LastQuery { get; private set; }

        public IQueryable<T> GetQueryable<T>() where T : Document
        {
            return new SolrQueryProvider<T>(this).GetQueryable();
        }

        public Response<T> Search<T>(string query) where T : Document
        {
            LastQuery = query;

            var documents = new List<TestDocument>
            {
                new TestDocument{ Title="test" },
                new TestDocument{ Title="test 1" },
                new TestDocument{ Title="test 2" }
            };
            return new Response<T>
            {

                ResponseNode = new ResponseNode<T>
                {
                    Found = documents.Count,
                    Documents = documents.Cast<T>()
                }
            };
        }
    }
}
