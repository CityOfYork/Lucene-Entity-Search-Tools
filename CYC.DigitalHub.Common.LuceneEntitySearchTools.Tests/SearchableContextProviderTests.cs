using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests.Helpers;
using Moq;
using System.Data.Entity;
using CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests.Models;
using System.Linq;
using CYC.DigitalHub.Common.LuceneEntitySearchTools;
using System.Collections.Generic;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests
{
    [TestClass]
    public class SearchableContextProviderTests
    {
        private TestDbContext context;
        
        [TestInitialize]
        public void Initialize()
        {
            context = new TestDbContext();
        }

        TestDataGenerator tdg = new TestDataGenerator();


        public SearchableContextProviderTests()
        {
            var data = tdg.AllTestUsers;
        }

        [TestMethod]
        public void AContextProviderCanIndexADatabase()
        {
            SearchableContextProvider<TestDbContext> searchProvider = new SearchableContextProvider<TestDbContext>();
            LuceneIndexerOptions options = new LuceneIndexerOptions(null, null, true);

            searchProvider.Initialize(options, context);

            searchProvider.CreateIndex();

            Assert.AreEqual(2000, searchProvider.IndexCount);

            // cleanup
            searchProvider.DeleteIndex();
        }

        [TestMethod]
        public void AContextCanBeSearchedUsingAContextProvider()
        {
            SearchableContextProvider<TestDbContext> searchProvider = new SearchableContextProvider<TestDbContext>();
            LuceneIndexerOptions options = new LuceneIndexerOptions(null, null, true);
            
            searchProvider.Initialize(options, context);

            searchProvider.CreateIndex();

            SearchOptions searchOptions = new SearchOptions("John", "FirstName");

            // test
            var results = searchProvider.ScoredSearch<User>(searchOptions);

            Assert.AreEqual(5, results.TotalHits);

            // cleanup
            searchProvider.DeleteIndex();
        }

        [TestMethod]
        public void SkipAndTakeWorkWhenSearchingUsingAContextProvider()
        {
            SearchableContextProvider<TestDbContext> searchProvider = new SearchableContextProvider<TestDbContext>();
            LuceneIndexerOptions options = new LuceneIndexerOptions(null, null, true);

            searchProvider.Initialize(options, context);

            searchProvider.CreateIndex();

            SearchOptions searchOptions = new SearchOptions("John", "FirstName");

            // test
            var initialResults = searchProvider.ScoredSearch<User>(searchOptions);
            int lastId = initialResults.Results[4].Entity.Id;

            Assert.AreEqual(5, initialResults.TotalHits);
            Assert.AreEqual(5, initialResults.Results.Count());

            searchOptions.Skip = 4;
            searchOptions.Take = 1;
            var subResults = searchProvider.ScoredSearch<User>(searchOptions);

            Assert.AreEqual(5, subResults.TotalHits);
            Assert.AreEqual(1, subResults.Results.Count());
            Assert.AreEqual(lastId, subResults.Results.First().Entity.Id);

            // cleanup
            searchProvider.DeleteIndex();
        }

        [TestMethod]
        public void AContextCanBeSearchedUsingAWildCard()
        {
            SearchableContextProvider<TestDbContext> searchProvider = new SearchableContextProvider<TestDbContext>();
            LuceneIndexerOptions options = new LuceneIndexerOptions(null, null, true);

            searchProvider.Initialize(options, context);

            searchProvider.CreateIndex();

            SearchOptions searchOptions = new SearchOptions("Joh*", "FirstName");

            // test
            var results = searchProvider.ScoredSearch<User>(searchOptions);

            Assert.AreEqual(10, results.TotalHits);

            // cleanup
            searchProvider.DeleteIndex();
        }

        [TestMethod]
        public void ASearchWillReturnTheSameResultsAsAScoredSearch()
        {
            SearchableContextProvider<TestDbContext> searchProvider = new SearchableContextProvider<TestDbContext>();
            LuceneIndexerOptions options = new LuceneIndexerOptions(null, null, true);

            searchProvider.Initialize(options, context);

            searchProvider.CreateIndex();

            SearchOptions searchOptions = new SearchOptions("Joh*", "FirstName");

            // test
            var results = searchProvider.Search<User>(searchOptions);

            Assert.AreEqual(10, results.TotalHits);

            // cleanup
            searchProvider.DeleteIndex();
        }

        [TestMethod]
        public void AScoredSearchWillOrderByRelevence()
        {
            SearchableContextProvider<TestDbContext> searchProvider = new SearchableContextProvider<TestDbContext>();
            LuceneIndexerOptions options = new LuceneIndexerOptions(null, null, true);

            searchProvider.Initialize(options, context);

            searchProvider.CreateIndex();

            SearchOptions searchOptions = new SearchOptions("Jeremy Burns", "FirstName,Surname");

            var results = searchProvider.ScoredSearch<User>(searchOptions);

            var first = results.Results.First().Entity;
            var highest = results.Results.First().Score;
            var lowest = results.Results.Last().Score;

            Assert.IsTrue(highest > lowest);
            Assert.AreEqual("Jeremy", first.FirstName);
            Assert.AreEqual("Burns", first.Surname);

            searchProvider.DeleteIndex();
        }

        [TestMethod]
        public void ASearchWillStillOrderByRelevence()
        {
            SearchableContextProvider<TestDbContext> searchProvider = new SearchableContextProvider<TestDbContext>();
            LuceneIndexerOptions options = new LuceneIndexerOptions(null, null, true);

            searchProvider.Initialize(options, context);

            searchProvider.CreateIndex();

            SearchOptions searchOptions = new SearchOptions("Jeremy Burns", "FirstName,Surname");

            var results = searchProvider.Search<User>(searchOptions);

            var first = results.Results.First();

            Assert.AreEqual("Jeremy", first.FirstName);
            Assert.AreEqual("Burns", first.Surname);

            searchProvider.DeleteIndex();
        }

        [TestMethod]
        public void ASearchCanIncludeAnOrderBy()
        {
            SearchableContextProvider<TestDbContext> searchProvider = new SearchableContextProvider<TestDbContext>();
            LuceneIndexerOptions options = new LuceneIndexerOptions(null, null, true);

            searchProvider.Initialize(options, context);
            searchProvider.CreateIndex();

            SearchOptions search = new SearchOptions("Jeremy", "FirstName", 5, null, null, "Surname");

            var results = searchProvider.ScoredSearch<User>(search);
        }

        [TestMethod]
        public void ASearchCanReturnMultipleEntityTypes()
        {
            SearchableContextProvider<TestDbContext> searchProvider = new SearchableContextProvider<TestDbContext>();
            LuceneIndexerOptions options = new LuceneIndexerOptions(null, null, true);
            searchProvider.Initialize(options, context);
            searchProvider.CreateIndex();

            SearchOptions query = new SearchOptions("John China", "FirstName,Country");

            var resultSet = searchProvider.ScoredSearch(query);

            Assert.AreNotEqual(resultSet.Results.First().Entity.Type, resultSet.Results.Last().Entity.Type);
        }

        [TestMethod]
        public void ASearchCanBeOrderedAcrossTypes()
        {
            SearchableContextProvider<TestDbContext> searchProvider = new SearchableContextProvider<TestDbContext>();
            LuceneIndexerOptions options = new LuceneIndexerOptions(null, null, true);
            searchProvider.Initialize(options, context);
            searchProvider.CreateIndex();

            SearchOptions query = new SearchOptions("Moore China", "Surname,Country");
            query.OrderBy.Add("Name");

            var resultSet = searchProvider.ScoredSearch(query);

            Assert.AreNotEqual(resultSet.Results.First().Entity.Type, resultSet.Results.Last().Entity.Type);
        }

        [TestMethod]
        public void ASearchCanOrderByMultipleFields()
        {
            SearchableContextProvider<TestDbContext> searchProvider = new SearchableContextProvider<TestDbContext>();
            LuceneIndexerOptions options = new LuceneIndexerOptions(null, null, true);

            User jc = new User()
            {
                FirstName = "John",
                Surname = "Chapman",
                JobTitle = "Test Engineer",
                IndexId = Guid.NewGuid(),
                Email = "john.chapman@test.com"
            };

            searchProvider.Initialize(options, context);

            context.Users.Add(jc);
            context.SaveChanges();

            searchProvider.CreateIndex();

            SearchOptions search = new SearchOptions("John", "FirstName", 1000, null, null, "Surname,JobTitle");

            var results = searchProvider.ScoredSearch<User>(search);

            var topResult = results.Results[0];
            var secondResult = results.Results[1];

            Assert.AreEqual("Sales Associate", topResult.Entity.JobTitle);
            Assert.AreEqual("Test Engineer", secondResult.Entity.JobTitle);

            searchProvider.DeleteIndex();
        }

        [TestMethod]
        public void SaveChangesUpdatesEntitiesAddedToTheIndex()
        {
            SearchableContextProvider<TestDbContext> searchProvider = new SearchableContextProvider<TestDbContext>();
            LuceneIndexerOptions options = new LuceneIndexerOptions(null, null, true);
            searchProvider.Initialize(options, context);
            searchProvider.CreateIndex();

            var newUser = new User()
            {
                FirstName = "Duke",
                Surname = "Nukem",
                Email = "duke.nukem@test.com",
                IndexId = Guid.NewGuid(),
                JobTitle = "Shooty Man"
            };

            var search = new SearchOptions("Nukem", "Surname");

            var initialResults = searchProvider.Search<User>(search);

            searchProvider.Context.Users.Add(newUser);
            searchProvider.SaveChanges(true);

            var newResults = searchProvider.Search<User>(search);

            Assert.AreEqual(0, initialResults.TotalHits);
            Assert.AreEqual(1, newResults.TotalHits);

            Assert.AreEqual(newUser.Id, newResults.Results[0].Id);
        }

        [TestMethod]
        public void SaveChangesDeletesRemovedEntitiesFromTheIndex()
        {
            SearchableContextProvider<TestDbContext> searchProvider = new SearchableContextProvider<TestDbContext>();
            LuceneIndexerOptions options = new LuceneIndexerOptions(null, null, true);
            searchProvider.Initialize(options, context);
            searchProvider.CreateIndex();

            var search = new SearchOptions("John", "FirstName");

            int initialIndexCount = searchProvider.IndexCount;

            var initialResults = searchProvider.Search<User>(search);
            int resultsCount = initialResults.TotalHits;

            // delete entities
            //
            // NOTE: Because the search result entities are attached to the context
            // we can easily do linq operations on them
            //
            searchProvider.Context.Users.RemoveRange(initialResults.Results);
            searchProvider.SaveChanges(true);

            var updatedSearch = searchProvider.Search<User>(search);
            int finalIndexCount = searchProvider.IndexCount;

            Assert.AreEqual(0, updatedSearch.TotalHits);
            Assert.AreEqual(0, updatedSearch.Results.Count);
            Assert.AreEqual((initialIndexCount - resultsCount), finalIndexCount);
        }


        [TestMethod]
        public void NonValidEntitiesAreIgnored()
        {
            SearchableContextProvider<MockNonIndexableContext> searchProvider = new SearchableContextProvider<MockNonIndexableContext>();

            LuceneIndexerOptions indexerOptions = new LuceneIndexerOptions(null, null, true);

            searchProvider.Initialize(indexerOptions);

            searchProvider.CreateIndex();

            Assert.AreEqual(0, searchProvider.IndexCount);
        }

    }
}
