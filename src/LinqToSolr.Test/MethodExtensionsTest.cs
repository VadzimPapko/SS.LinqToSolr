using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS.LinqToSolr.Extensions;
using System;
using System.Configuration;
using System.Linq;

namespace SS.LinqToSolr.Test
{
    [TestClass]
    public class MethodExtensionsTest : IDisposable
    {
        private SolrService _api;
        public MethodExtensionsTest()
        {
            _api = new SolrService(ConfigurationManager.AppSettings["Solr.Url"], "sitecore_signals");
        }

        [TestMethod]
        public void FuzzyQuery()
        {
            var term = "test";
            var fuzzy1 = _api.GetContext<TestDocument>().Where(x => x.Title.Fuzzy(term, 0.8f)).ToString();
            Assert.AreEqual(fuzzy1, "q=(title_s:test~0.8)");
            var fuzzy2 = _api.GetContext<TestDocument>().Where(x => x.Title.Fuzzy(term)).ToString();
            Assert.AreEqual(fuzzy2, "q=(title_s:test~)");
            var fuzzy3 = _api.GetContext<TestDocument>().Query(term.Fuzzy()).ToString();
            Assert.AreEqual(fuzzy3, "q=(test~)");
        }

        [TestMethod]
        public void ProximityQuery()
        {
            var term = "test test";
            var proximity = _api.GetContext<TestDocument>().Where(x => x.Title.Proximity(term, 10)).ToString();
            Assert.AreEqual(proximity, "q=(title_s:\"test test\"~10)");
            var proximity2 = _api.GetContext<TestDocument>().Query(term.Proximity(10)).ToString();
            Assert.AreEqual(proximity2, "q=(\"test test\"~10)");
        }

        [TestMethod]
        public void BoostQuery()
        {
            var boost1 = _api.GetContext<TestDocument>().Where(x => (x.Title== "test").Boost(2)).ToString();
            Assert.AreEqual(boost1, "q=((title_s:test)^2)");
            var boost2 = _api.GetContext<TestDocument>().Where(x => (x.Title == "test").Boost(2.5f) && (x.Type == "type 1").Boost(2)).ToString();
            Assert.AreEqual(boost2, "q=((title_s:test)^2.5 AND (_type:\"type 1\")^2)");
        }

        [TestMethod]
        public void ConstantScore()
        {
            var constantScore = _api.GetContext<TestDocument>().Where(x => (x.Title == "test").ConstantScore(1.5f)).ToString();
            Assert.AreEqual(constantScore, "q=((title_s:test)^=1.5)");
            var constantScore2 = _api.GetContext<TestDocument>().Where(x => (x.Title == "test" || x.Title == "test1").ConstantScore(1.5f)).ToString();
            Assert.AreEqual(constantScore2, "q=((title_s:test OR title_s:test1)^=1.5)");
        }

        #region Disposing
        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _api.Dispose();
            }

            _disposed = true;
        }
        #endregion
    }
}
