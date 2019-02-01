using System;
using System.Collections.Generic;
using System.Linq;
using SS.LinqToSolr.Models;
using SS.LinqToSolr.Models.SearchResponse;
using SS.LinqToSolr.Test.Models;
using SS.LinqToSolr.Translators;

namespace SS.LinqToSolr.Test
{
    public class FakeSearchContext : ISearchContext
    {
        public string LastQuery { get; private set; }

        public IQueryable<T> GetQueryable<T>()
        {
            return new SolrQueryProvider<T>(this, new NodeTranslator(new NewtonsoftJsonFieldTranslator())).GetQueryable();
        }

        public Response<T> Search<T>(string query)
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
