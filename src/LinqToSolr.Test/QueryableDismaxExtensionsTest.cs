using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using SS.LinqToSolr.Extensions;
using SS.LinqToSolr.Test.Models;
using System.Linq;
using SS.LinqToSolr.Common;

namespace SS.LinqToSolr.Test
{
    [TestClass]
    public class QueryableDismaxExtensionsTest : BaseSolrTest
    {
        public QueryableDismaxExtensionsTest() : base("sitecore_signals")
        {
        }

        [TestMethod]
        public void Dismax()
        {
            var dismax = _api.GetQueryable<TestDocument>().Dismax().ToString();
            Assert.AreEqual(dismax, "q=&q.alt=*:*&defType=dismax");
        }

        [TestMethod]
        public void QueryFields()
        {
            var dismax = _api.GetQueryable<TestDocument>().Dismax().ToString();
            Assert.AreEqual(dismax, "q=&defType=dismax");
        }

        [TestMethod]
        public void BoostQuery()
        {
            var dismax = _api.GetQueryable<TestDocument>().Dismax().BoostQuery(x => (x.DocumentId == "f40b02a9-81ca-4b72-8513-be0229299e40").Boost(1.5f)).ToString();
            Assert.AreEqual(dismax, "q=&q.alt=*:*&bq=(_documentid:f40b02a9-81ca-4b72-8513-be0229299e40)^1.5&defType=dismax");

            var predicate = PredicateBuilder.False<TestDocument>();
            var list = new[] { "test1", "test2" };
            foreach (var i in list)
                predicate = predicate.Or(p => p.Title == i);
            var dismax2 = _api.GetQueryable<TestDocument>().Dismax().BoostQuery(predicate).ToString();
            Assert.AreEqual(dismax2, "q=&q.alt=*:*&bq=title_s:test1 OR title_s:test2&defType=dismax");
        }

        [TestMethod]
        public void QueryAlt()
        {
            var dismax = _api.GetQueryable<TestDocument>().Dismax().DismaxQueryAlt(x => x.Id == "f40b02a9-81ca-4b72-8513-be0229299e40").ToString();
            Assert.AreEqual(dismax, "q=&q.alt=_uniqueid:f40b02a9-81ca-4b72-8513-be0229299e40&defType=dismax");
        }

        [TestMethod]
        public void Min()
        {
            var dismax = _api.GetQueryable<TestDocument>().Dismax().ToString();
            Assert.AreEqual(dismax, "q=&defType=dismax");
        }
    }
}
