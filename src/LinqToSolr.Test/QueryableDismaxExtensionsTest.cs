using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using SS.LinqToSolr.Extensions;
using SS.LinqToSolr.Test.Models;
using System.Linq;

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
            var dismax = _api.GetQueryable<TestDocument>().Dismax(x => x).ToString();
            Assert.AreEqual(dismax, "q=&defType=dismax");
        }

        [TestMethod]
        public void QueryFields()
        {
            var dismax = _api.GetQueryable<TestDocument>().Dismax(x => x).ToString();
            Assert.AreEqual(dismax, "q=*:*&defType=dismax");
        }

        [TestMethod]
        public void BoostQuery()
        {
            var dismax = _api.GetQueryable<TestDocument>().Dismax(x => x.BoostQuery(bq => bq.Where(d => (d.DocumentId == "f40b02a9-81ca-4b72-8513-be0229299e40").Boost(1.5f)))).ToString();
            Assert.AreEqual(dismax, "q=&bq=((_documentid:f40b02a9-81ca-4b72-8513-be0229299e40)^1.5)&defType=dismax");
        }

        [TestMethod]
        public void QueryAlt()
        {
            var dismax = _api.GetQueryable<TestDocument>().Dismax(x => x.DismaxQueryAlt(a=>a.Where(d=>d.Id == "f40b02a9-81ca-4b72-8513-be0229299e40"))).ToString();
            Assert.AreEqual(dismax, "q=&q.alt=(_uniqueid:f40b02a9-81ca-4b72-8513-be0229299e40)&defType=dismax");
        }

        [TestMethod]
        public void Min()
        {
            var dismax = _api.GetQueryable<TestDocument>().Dismax(x => x).ToString();
            Assert.AreEqual(dismax, "q=*:*&defType=dismax");
        }
    }
}
