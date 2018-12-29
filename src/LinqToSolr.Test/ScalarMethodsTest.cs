using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System;
using SS.LinqToSolr.Test.Models;
using System.Configuration;

namespace SS.LinqToSolr.Test
{
    [TestClass]
    public class ScalarMethodsTest
    {
        protected FakeSearchContext _api;
        public ScalarMethodsTest()
        {
            _api = new FakeSearchContext();
        }

        [TestMethod]
        public void Fist()
        {
            var first = _api.GetQueryable<TestDocument>().Where(x => x.Title == "smth").First();
            Assert.AreEqual(_api.LastQuery, "q=(title_s:smth)&rows=1");
            Assert.AreEqual(first.Title, "test");
            var first2 = _api.GetQueryable<TestDocument>().First(x => x.Title == "smth");
            Assert.AreEqual(_api.LastQuery, "q=(title_s:smth)&rows=1");
            Assert.AreEqual(first2.Title, "test");
        }

        [TestMethod]
        public void FirstOrDefault()
        {
            var first = _api.GetQueryable<TestDocument>().Where(x => x.Title == "smth").FirstOrDefault();
            Assert.AreEqual(_api.LastQuery, "q=(title_s:smth)&rows=1");
            Assert.AreEqual(first.Title, "test");
            var first2 = _api.GetQueryable<TestDocument>().FirstOrDefault(x => x.Title == "smth");
            Assert.AreEqual(_api.LastQuery, "q=(title_s:smth)&rows=1");
            Assert.AreEqual(first2.Title, "test");
        }

        //[TestMethod]
        //public void Last()
        //{
        //    var last = _api.GetQueryable<TestDocument>().Where(x => x.Title == "smth").Last();
        //    Assert.AreEqual(_api.LastQuery, "q=(title_s:smth)&rows=1");
        //    Assert.AreEqual(last.Title, "test");
        //    var last2 = _api.GetQueryable<TestDocument>().Last(x => x.Title == "smth");
        //    Assert.AreEqual(_api.LastQuery, "q=(title_s:smth)&rows=1");
        //    Assert.AreEqual(last2.Title, "test");
        //}

        //[TestMethod]
        //public void LastOrDefault()
        //{
        //    var last = _api.GetQueryable<TestDocument>().Where(x => x.Title == "smth").LastOrDefault();
        //    Assert.AreEqual(_api.LastQuery, "q=(title_s:smth)&rows=1");
        //    Assert.AreEqual(last.Title, "test");
        //    var last2 = _api.GetQueryable<TestDocument>().LastOrDefault(x => x.Title == "smth");
        //    Assert.AreEqual(_api.LastQuery, "q=(title_s:smth)&rows=1");
        //    Assert.AreEqual(last2.Title, "test");
        //}

        [TestMethod]
        public void Single()
        {
            var single = _api.GetQueryable<TestDocument>().Where(x => x.Title == "smth").Single();
            Assert.AreEqual(_api.LastQuery, "q=(title_s:smth)&rows=1");
            Assert.AreEqual(single.Title, "test");
            var single2 = _api.GetQueryable<TestDocument>().Last(x => x.Title == "smth");
            Assert.AreEqual(_api.LastQuery, "q=(title_s:smth)&rows=1");
            Assert.AreEqual(single2.Title, "test");
        }

        [TestMethod]
        public void SingleOrDefault()
        {
            var single = _api.GetQueryable<TestDocument>().Where(x => x.Title == "smth").SingleOrDefault();
            Assert.AreEqual(_api.LastQuery, "q=(title_s:smth)&rows=1");
            Assert.AreEqual(single.Title, "test");
            var single2 = _api.GetQueryable<TestDocument>().LastOrDefault(x => x.Title == "smth");
            Assert.AreEqual(_api.LastQuery, "q=(title_s:smth)&rows=1");
            Assert.AreEqual(single2.Title, "test");
        }

        [TestMethod]
        public void Count()
        {
            var count = _api.GetQueryable<TestDocument>().Where(x => x.Title == "smth").Count();
            Assert.AreEqual(_api.LastQuery, "q=(title_s:smth)");
            Assert.AreEqual(count, 3);
            var count2 = _api.GetQueryable<TestDocument>().Count();
            Assert.AreEqual(_api.LastQuery, "q=*:*");
            Assert.AreEqual(count2, 3);
            var count3 = _api.GetQueryable<TestDocument>().Count(x => x.Title == "smth");
            Assert.AreEqual(_api.LastQuery, "q=(title_s:smth)");
            Assert.AreEqual(count3, 3);
        }
    }
}
