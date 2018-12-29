using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using SS.LinqToSolr.Test.Models;

namespace SS.LinqToSolr.Test
{
    [TestClass]
    public class QueryWithPredicateBuilderTest : BaseSolrTest
    {
        public QueryWithPredicateBuilderTest() : base("sitecore_signals")
        {
        }

        [TestMethod]
        public void Or()
        {
            var predicate = PredicateBuilder.False<TestDocument>();
            var list = new[] { "test1", "test2" };
            foreach (var i in list)
                predicate = predicate.Or(p => p.Title == i);
            var query = _api.GetContext<TestDocument>().Where(predicate).ToString();
            Assert.AreEqual(query, "q=(title_s:test1 OR title_s:test2)");
        }

        [TestMethod]
        public void And()
        {
            var predicate = PredicateBuilder.True<TestDocument>();
            var list = new[] { "test1", "test2" };
            foreach (var i in list)
                predicate = predicate.And(p => p.Title == i);
            var query = _api.GetContext<TestDocument>().Where(predicate).ToString();
            Assert.AreEqual(query, "q=(title_s:test1 AND title_s:test2)");
        }
    }
}
