using CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools
{
    /// <summary>
    /// SearchContextProvider provides a wrapper around any context that implements IDbContext to allow easy
    /// searching and indexing functions to the context
    /// </summary>
    /// <typeparam name="CTX">The DbContext or any class that implements IDbContext</typeparam>
    public class SearchableContextProvider<CTX> : ISearchableContextProvider<CTX> where CTX : IDbContext, new()
    {
        public CTX Context { get; protected set; }
        private IDbContext contextInterface;

        private static Directory directory;
        private static Analyzer analyzer;

        private static LuceneIndexer indexer;
        private static LuceneIndexSearcher searcher;

        private static bool isInitialized = false;

        public void Initialize(LuceneIndexerOptions indexerOptions, bool overrideIfExists = false)
        {
            if(isInitialized == false || overrideIfExists == true)
            {
                initializeLucene(indexerOptions);
            }

            Context = new CTX();
            contextInterface = Context as IDbContext;
        }

        public void Initialize(LuceneIndexerOptions indexerOptions, CTX context, bool overrideIfExists = false)
        {
            if (isInitialized == false || overrideIfExists == true)
            {
                initializeLucene(indexerOptions);
            }

            Context = context;
            contextInterface = Context as IDbContext;
        }

        /// <summary>
        /// Initialize Lucene
        /// Sets up the directory, analyzer, indexer and searcher
        /// </summary>
        /// <param name="options"></param>
        private void initializeLucene(LuceneIndexerOptions options)
        {
            // create the directory
            if (options.UseRamDirectory == true)
            {
                directory = new RAMDirectory();
            }
            else
            {
                if(directory == null)
                {
                    directory = FSDirectory.Open(options.Path);
                }
                
            }

            // create the analyzer
            analyzer = new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30);

            // create the indexer
            indexer = new LuceneIndexer(directory, analyzer, options.MaximumFieldLength);

            // create the searcher
            searcher = new LuceneIndexSearcher(directory, analyzer);
        }

        /// <summary>
        /// Examines the context changes from the ChangeTracker
        /// and returns a collection of LuceneIndexChanges
        /// </summary>
        /// <returns>LuceneIndexChangeset - collection of entity changes converted to LuceneIndexChanges</returns>
        private LuceneIndexChangeset GetChangeset()
        {
            LuceneIndexChangeset changes = new LuceneIndexChangeset();

            foreach(var entity in contextInterface.ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged))
            {
                Type entityType = entity.Entity.GetType();
                bool implementsILuceneIndexable = typeof(ILuceneIndexable).IsAssignableFrom(entityType);
                if(implementsILuceneIndexable == true)
                {
                    MethodInfo method = entityType.GetMethod("ToDocument");
                    if(method != null)
                    {
                        LuceneIndexChange change = new LuceneIndexChange(entity.Entity as ILuceneIndexable);

                        // If the entity doesn't have a guid IndexId, then add one in
                        if ((change.Entity.IndexId == Guid.Empty || change.Entity.IndexId == null)
                            && (change.State == LuceneIndexState.Added || change.State == LuceneIndexState.Updated))
                        {
                            change.Entity.IndexId = Guid.NewGuid();
                        }

                        switch(entity.State)
                        {
                            case EntityState.Added:
                                change.State = LuceneIndexState.Added;
                                break;
                            case EntityState.Deleted:
                                change.State = LuceneIndexState.Removed;
                                break;
                            case EntityState.Modified:
                                change.State = LuceneIndexState.Updated;
                                break;
                            default:
                                change.State = LuceneIndexState.Unchanged;
                                break;
                        }
                        changes.Entries.Add(change);        
                    }
                }
            }

            return changes;
        }

        /// <summary>
        /// Gets the concrete version of the document
        /// </summary>
        /// <param name="doc">The Document to convert</param>
        /// <returns>The concrete type as an ILuceneIndexable</returns>
        private ILuceneIndexable GetConcreteFromDocument(Document doc)
        {
            // get the object
            Type t = Type.GetType(doc.Get("Type"));
            int id = int.Parse(doc.Get("Id"));

            // get the DbSet
            var set = contextInterface.Set(t);

            // try and find the entity
            var entity = set.Find(id);

            // return the entity as an ILuceneIndexable
            return entity as ILuceneIndexable;
        }

        /// <summary>
        /// Call the SaveChanges method on the context
        /// and index any changes
        /// </summary>
        /// <returns>Value passed back from the SaveChanges method on the underlying context</returns>
        public int SaveChanges(bool index = true)
        {
            int result = 0;

            if(contextInterface.ChangeTracker.HasChanges())
            {
                // get the Lucene Changeset from the current ChangeTracker
                LuceneIndexChangeset changes = GetChangeset();

                // Call the base AFTER we have collected out changeset
                result = contextInterface.SaveChanges();

                // finally, update our lucene with the changes if there are any
                if(changes.HasChanges && index == true)
                {
                    indexer.Update(changes);
                }
            }

            return result;
        }

        public async Task<int> SaveChangesAsync(bool index = true)
        {
            int result = 0;

            if(contextInterface.ChangeTracker.HasChanges())
            {
                // get the chages
                LuceneIndexChangeset changes = GetChangeset();

                // call the base async SaveChangesAsync method
                result = await contextInterface.SaveChangesAsync();

                if(changes.HasChanges == true && index == true)
                {
                    indexer.Update(changes);
                }
            }

            return result;
        }

        /// <summary>
        /// Return the number of documents in the index
        /// </summary>
        public int IndexCount { get { return indexer.Count(); } }

        public void DeleteIndex()
        {
            if(indexer != null)
            {
                indexer.DeleteAll();
            }
        }

        /// <summary>
        /// Scan the context and index all entities that implement ILuceneIndexable and have properties that require indexing
        /// </summary>
        /// <param name="optimize">Whether to optimize the index once completed</param>
        public void CreateIndex(bool optimize = false)
        {
            if(indexer != null)
            {
                // the new index
                List<ILuceneIndexable> index = new List<ILuceneIndexable>();

                // get a list of the context properties
                PropertyInfo[] properties = Context.GetType().GetProperties();

                // build a collection of properties that can be indexed
                foreach(PropertyInfo pi in properties)
                {
                    // check if the entity is compatible
                    if(typeof(IEnumerable<ILuceneIndexable>).IsAssignableFrom(pi.PropertyType))
                    {
                        // get all the entities for that type
                        var entities = Context.GetType().GetProperty(pi.Name).GetValue(Context, null);

                        // add them to the index
                        index.AddRange(entities as IEnumerable<ILuceneIndexable>);
                    }
                }

                // create the index, but only if we have any entities
                if(index.Any())
                {
                    indexer.CreateIndex(index, true, optimize);
                }
            }
        }
            
        /// <summary>
        /// Typed search - Perform a search and restrict the results to a particular type
        /// Search results are converted to the type in question before being returned
        /// NOTE: No scoring information is returned
        /// </summary>
        /// <typeparam name="T">The entity Type to search within - NOTE: Must implement ILuceneIndexable</typeparam>
        /// <param name="options">SearchOptions that represent the query</param>
        /// <returns></returns>
        public ISearchResultCollection<T> Search<T>(SearchOptions options)
        {
            options.Type = typeof(T);
            var indexResults = searcher.ScoredSearch(options);

            ISearchResultCollection<T> resultSet = new SearchResultCollection<T>() { TotalHits = indexResults.TotalHits };

            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach(var indexResult in indexResults.Results)
            {
                T entity = (T)GetConcreteFromDocument(indexResult.Document);
                resultSet.Results.Add(entity);
            }
            sw.Stop();
            resultSet.TimeTaken = indexResults.TimeTaken + sw.ElapsedMilliseconds;

            return resultSet;
        }
            
        public IScoredSearchResultCollection<T> ScoredSearch<T>(SearchOptions options)
        {
            // make our search a typed search if we haven't asked for an ILuceneIndexable
            if (typeof(T) != typeof(ILuceneIndexable)) { options.Type = typeof(T); }

            var indexResults = searcher.ScoredSearch(options);

            IScoredSearchResultCollection<T> results = new ScoredSearchResultCollection<T>();
            results.TotalHits = indexResults.TotalHits;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            foreach(var indexResult in indexResults.Results)
            {
                IScoredSearchResult<T> result = new ScoredSearchResult<T>();
                result.Score = indexResult.Score;
                result.Entity = (T)GetConcreteFromDocument(indexResult.Document);
                results.Results.Add(result);
            }
            sw.Stop();
            results.TimeTaken = indexResults.TimeTaken + sw.ElapsedMilliseconds;

            return results;
        }

        public IScoredSearchResultCollection<ILuceneIndexable> ScoredSearch(SearchOptions options)
        {
            return ScoredSearch<ILuceneIndexable>(options);
        }

        public ISearchResultCollection<ILuceneIndexable> Search(SearchOptions options)
        {
            return Search<ILuceneIndexable>(options);
        }
    }
}
