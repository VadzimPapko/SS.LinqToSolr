using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;
using SS.LinqToSolr.Test.Models;
using Sitecore.ContentSearch.Linq;
using Sitecore.ContentSearch;

namespace SS.LinqToSolr.Test
{
    [TestClass]
    public class SitecoreTest : BaseSitecoreTest
    {
        public SitecoreTest() : base("sitecore_signals")
        {
        }

        [TestMethod]
        public void InContext()
        {
            var emptyQuery = _api.GetQueryable<TestDocument>().Where(x=>x.Title == "hi").InContext(new CultureExecutionContext(new System.Globalization.CultureInfo("ru-RU"))).ToString();
            Assert.AreEqual(emptyQuery, "q=title_s_ru:hi");
        }
    }
}
