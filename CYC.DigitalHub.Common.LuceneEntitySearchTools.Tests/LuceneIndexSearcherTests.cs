using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Lucene.Net.Store;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests.Helpers;
using System.Collections.Generic;
using System.Linq;
using CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests.Models;
using Lucene.Net.Documents;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests
{
    [TestClass]
    public class LuceneIndexSearcherTests
    {
        // class setup
        static Directory directory = new RAMDirectory();
        static Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
        static TestDataGenerator tdg = new TestDataGenerator();

        // create an index
        static LuceneIndexer indexer = new LuceneIndexer(directory, analyzer);
        static LuceneIndexSearcher searcher = new LuceneIndexSearcher(directory, analyzer);

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            indexer.CreateIndex(tdg.AllData);
        }

        [TestMethod]
        public void AnIndexCanBeSearched()
        {
            SearchOptions options = new SearchOptions("John", "FirstName");

            var results = searcher.ScoredSearch(options);

            Assert.AreEqual(5, results.TotalHits);
        }

        [TestMethod]
        public void SearchesCanBeDoneAcrossMultipleTypes()
        {
            SearchOptions options = new SearchOptions("John China", "FirstName, Country");
            var results = searcher.ScoredSearch(options);

            var firstType = results.Results.First().Document.GetField("Type");
            var lastType = results.Results[results.TotalHits - 1].Document.GetField("Type");

            Assert.AreNotEqual(firstType, lastType);
        }

        [TestMethod]
        public void TopNNumberOfResultsCanBeReturned()
        {
            SearchOptions options = new SearchOptions("China", "Country", 1000, null, typeof(City));

            var allResults = searcher.ScoredSearch(options);

            options.Take = 10;

            var subSet = searcher.ScoredSearch(options);

            for(var index = 0; index < 10; index++)
            {
                Assert.AreEqual(allResults.Results[index].Document.Get("IndexId"), subSet.Results[index].Document.Get("IndexId"));
            }

            Assert.AreEqual(10, subSet.Results.Count());
            Assert.AreEqual(allResults.TotalHits, subSet.TotalHits);  
        }

        [TestMethod]
        public void ResultsetCanBeSkippedAndTaken()
        {
            SearchOptions options = new SearchOptions("China", "Country", 1000, null, typeof(City));

            var allResults = searcher.ScoredSearch(options);

            options.Take = 10;
            options.Skip = 10;

            var subSet = searcher.ScoredSearch(options);

            for (var index = 0; index < 10; index++)
            {
                Assert.AreEqual(allResults.Results[index + 10].Document.Get("IndexId"), subSet.Results[index].Document.Get("IndexId"));
            }

            Assert.AreEqual(10, subSet.Results.Count());
            Assert.AreEqual(allResults.TotalHits, subSet.TotalHits);
        }

        [TestMethod]
        public void ResultsetCanBeOrdered()
        {
            SearchOptions options = new SearchOptions("John", "FirstName", 1000, null, typeof(User));

            var unordered = searcher.ScoredSearch(options);

            options.OrderBy.Add("Surname");

            var ordered = searcher.ScoredSearch(options);

            Assert.AreEqual(ordered.TotalHits, unordered.TotalHits);
            Assert.AreNotEqual(ordered.Results.First().Document.Get("Id"), unordered.Results.First().Document.Get("Id"));
        }

        [TestMethod]
        public void ASingleDocumentIsReturnedFromScoredSearchSingle()
        {
            SearchOptions options = new SearchOptions("jfisherj@alexa.com", "Email");

            var result = searcher.ScoredSearchSingle(options);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Document));
            Assert.AreEqual("jfisherj@alexa.com", result.Get("Email"));
        }

        [TestMethod]
        public void MultipleResultsIsNotAProblemFromScoredSearchSingle()
        {
            SearchOptions options = new SearchOptions("John", "FirstName");

            var result = searcher.ScoredSearchSingle(options);

            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(Document));
            Assert.AreEqual("John", result.Get("FirstName"));
        }


    }
}
