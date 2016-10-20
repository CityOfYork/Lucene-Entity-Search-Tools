using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests.Helpers;
using CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces;
using Lucene.Net.Store;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using System.Linq;
using CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests.Models;
using Lucene.Net.Documents;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests
{
    [TestClass]
    public class LuceneIndexerTests
    {
        TestDataGenerator tdg = new TestDataGenerator();
        Directory directory = new RAMDirectory();
        Analyzer analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

        private LuceneIndexer indexer;

        [TestInitialize]
        public void Initialize()
        {
            indexer = new LuceneIndexer(directory, analyzer);
            indexer.CreateIndex(tdg.AllData, true);
        }

        [TestMethod]
        public void AnIndexCanBeCreated()
        {           
            Assert.AreEqual(2000, indexer.Count());      
        }

        [TestMethod]
        public void AnIndexCanBeDeleted()
        {
            Assert.AreEqual(2000, indexer.Count());

            indexer.DeleteAll(true);

            Assert.AreEqual(0, indexer.Count());
        }

        [TestMethod]
        public void AnItemCanBeAddedToTheIndex()
        {
            Assert.AreEqual(2000, indexer.Count());

            indexer.Add(tdg.ANewUser());

            Assert.AreEqual(2001, indexer.Count());
        }

        [TestMethod]
        public void AnItemCanBeRemovedFromTheIndex()
        {
            indexer.Delete(tdg.AllData.First());

            Assert.AreEqual(1999, indexer.Count());
        }

        [TestMethod]
        public void AnItemCanBeUpdatedInTheIndex()
        {

            // we need a searcher for this test
            LuceneIndexSearcher searcher = new LuceneIndexSearcher(directory, analyzer);

            // get the 1st item
            SearchOptions options = new SearchOptions("ghudson0@rambler.ru", "Email");

            var initialResults = searcher.ScoredSearch(options);

            Assert.AreEqual(1, initialResults.TotalHits);

            Document rambler = initialResults.Results.First().Document;

            // convert to ILuceneIndexable
            User user = new User()
            {
                Id = int.Parse(rambler.Get("Id")),
                IndexId = new Guid(rambler.Get("IndexId")),
                FirstName = rambler.Get("FirstName"),
                Surname = rambler.Get("Surname"),
                Email = rambler.Get("Email"),
                JobTitle = rambler.Get("JobTitle")
            };

            // make an edit
            user.FirstName = "Duke";
            user.Surname = "Nukem";

            // add the update to the indexer
            indexer.Update(user);

            // search again
            var endResults = searcher.ScoredSearch(options);


            Assert.AreEqual(1, endResults.TotalHits);
            Assert.AreEqual(user.IndexId.ToString(), endResults.Results.First().Document.Get("IndexId"));
            Assert.AreEqual(user.Id.ToString(), endResults.Results.First().Document.Get("Id"));
        }
    }
}
