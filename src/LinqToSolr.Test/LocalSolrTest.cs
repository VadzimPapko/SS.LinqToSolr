using Microsoft.VisualStudio.TestTools.UnitTesting;
using SS.LinqToSolr.Test.Models;
using SS.LinqToSolr.Extensions;

namespace SS.LinqToSolr.Test
{
    [TestClass]
    public class LocalSolrTest : BaseSolrTest
    {
        public LocalSolrTest() : base("sitecore_signals")
        {
        }

        [TestMethod]
        public void GetResponse()
        {
            var response = _api.GetQueryable<TestDocument>().GetResponse();
            Assert.IsNotNull(response);
        }      
    }
}
