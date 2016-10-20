using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests.Helpers;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests
{
    [TestClass]
    public class LuceneSearchOptionsTests
    {
        [TestMethod]
        public void DefaultLuceneSearchOptionsInitialisesCollections()
        {
            SearchOptions options = new SearchOptions();

            Assert.IsNotNull(options.OrderBy);
            Assert.IsNotNull(options.Fields);
        }

        [TestMethod]
        public void LuceneSearchOptionsCanBeConstructedWithMultipleFields()
        {
            SearchOptions options = new SearchOptions("Test", "one,two,three,four");

            Assert.AreEqual(4, options.Fields.Count);
            Assert.AreEqual("one", options.Fields[0]);
            Assert.AreEqual("two", options.Fields[1]);
        }

        [TestMethod]
        public void LuceneSearchOptionsParsesFieldsAndOrderBy()
        {
            SearchOptions options = new SearchOptions("Test", "one, two  ,  three", 1000, null, null, "test, another test");

            Assert.AreEqual(3, options.Fields.Count);
            Assert.AreEqual("one", options.Fields[0]);
            Assert.AreEqual("two", options.Fields[1]);
            Assert.AreEqual("three", options.Fields[2]);

            Assert.AreEqual(2, options.OrderBy.Count);
            Assert.AreEqual("test", options.OrderBy[0]);
            Assert.AreEqual("anothertest", options.OrderBy[1]);
        }

        [TestMethod]
        public void GetBoostsWillReturnAValidSetOfBoostsForGivenOptions()
        {
            SearchOptions options = new SearchOptions("John Developer", "FirstName,JobTitle");

            Assert.AreEqual(2, options.Boosts.Count);
            Assert.AreEqual(1, options.Boosts["FirstName"]);
            Assert.AreEqual(1, options.Boosts["JobTitle"]);
        }

        [TestMethod]
        public void ABoostCanBeAdded()
        {
            SearchOptions options = new SearchOptions("Test", "One,Two,Three");

            options.SetBoost("Two", 2f);

            Assert.AreEqual(3, options.Boosts.Count);
            Assert.AreEqual(2, options.Boosts["Two"]);
        }

        [TestMethod]
        public void ClearingBoostsWillReturnDefaultValues()
        {
            Dictionary<string, float> boosts = new Dictionary<string, float>();

            boosts.Add("One", 1.1f);
            boosts.Add("Two", 9.1f);

            SearchOptions options = new SearchOptions("Test", "One,Two", 1000, boosts);

            Assert.AreEqual(2, options.Boosts.Count);
            Assert.AreEqual(1.1f, options.Boosts["One"]);
            Assert.AreEqual(9.1f, options.Boosts["Two"]);

            options.ClearBoosts();

            Assert.AreEqual(2, options.Boosts.Count);
            Assert.AreEqual(1, options.Boosts["One"]);
            Assert.AreEqual(1, options.Boosts["Two"]);
        }
    }
}
