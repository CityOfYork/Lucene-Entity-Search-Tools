using CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools
{
    public class LuceneIndexSearcher : ILuceneIndexSearcher
    {
        private static Directory directory;
        private static Analyzer analyzer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="directory">Lucene directory</param>
        /// <param name="analyzer">Lucene StandardAnalyzer</param>
        public LuceneIndexSearcher(Directory directory, Analyzer analyzer)
        {
            LuceneIndexSearcher.directory = directory;
            LuceneIndexSearcher.analyzer = analyzer;
        }

        private ILuceneSearchResultCollection PerformSearch(SearchOptions options, bool safeSearch)
        {
            // Results collection
            ILuceneSearchResultCollection results = new LuceneSearchResultCollection();

            using (var reader = IndexReader.Open(directory, true))
            using (var searcher = new IndexSearcher(reader))
            {
                Query query;

                // calculate the scores - this has a cpu hit!!!
                searcher.SetDefaultFieldSortScoring(true, true);

                // Escape our search if we're performing a safe search
                if (safeSearch == true) { options.SearchText = QueryParser.Escape(options.SearchText); }

                if (options.Fields.Count() == 1)
                {
                    // Single field search
                    QueryParser queryParser = new QueryParser(Lucene.Net.Util.Version.LUCENE_30, options.Fields[0], analyzer);
                    query = queryParser.Parse(options.SearchText);
                }
                else
                {
                    // Parse the boosts against the fields

                    // Multifield search
                    MultiFieldQueryParser multiFieldQueryParser = new MultiFieldQueryParser(Lucene.Net.Util.Version.LUCENE_30, options.Fields.ToArray(), analyzer, options.Boosts);
                    query = multiFieldQueryParser.Parse(options.SearchText);
                }

                List<SortField> sortFields = new List<SortField>();
                sortFields.Add(SortField.FIELD_SCORE);

                // if we have any sort fields then add them
                foreach (var sortField in options.OrderBy)
                {
                    sortFields.Add(new SortField(sortField, SortField.STRING));
                }

                // create our sort
                Sort sort = new Sort(sortFields.ToArray());

                ScoreDoc[] matches = searcher.Search(query, null, options.MaximumNumberOfHits, sort).ScoreDocs;

                results.TotalHits = matches.Count();

                // perform skip and take if needed
                if (options.Skip.HasValue)
                {
                    matches = matches.Skip(options.Skip.Value).ToArray();
                }
                if(options.Take.HasValue)
                {
                    matches = matches.Take(options.Take.Value).ToArray();
                }

                // create a list of documents from the results
                foreach (var match in matches)
                {
                    var id = match.Doc;
                    var doc = searcher.Doc(id);

                    // filter out unwanted documents if a type has been set
                    if (options.Type != null)
                    {
                        var t = doc.Get("Type");
                        if (options.Type.AssemblyQualifiedName == t)
                        {
                            results.Results.Add(new LuceneSearchResult()
                            {
                                Score = match.Score,
                                Document = doc
                            });
                        }
                    }
                    else
                    {
                        results.Results.Add(new LuceneSearchResult()
                        {
                            Score = match.Score,
                            Document = doc
                        });
                    }
                }

            }

            return results;
        }

        public Document ScoredSearchSingle(SearchOptions options)
        {
            options.MaximumNumberOfHits = 1;
            var results = ScoredSearch(options);
            
            if(results.TotalHits > 0)
            {
                return results.Results.First().Document;
            }
            else
            {
                return null;
            }
        }

        public ILuceneSearchResultCollection ScoredSearch(SearchOptions options)
        {
            // record performance
            Stopwatch sw = new Stopwatch();
            sw.Start();

            ILuceneSearchResultCollection results;

            try
            {
                results = PerformSearch(options, false);
            }
            catch (ParseException)
            {
                // perform a safe search
                results = PerformSearch(options, true);
            }

            sw.Stop();
            results.TimeTaken = sw.ElapsedMilliseconds;

            return results;
        }

        public ILuceneSearchResultCollection ScoredSearch(
            string searchText,
            string fields,
            int maximumNumberOfHits,
            Dictionary<string, float> boosts,
            Type type,
            string sortBy,
            int? skip,
            int? take
            )
        {
            SearchOptions options = new SearchOptions(
                searchText,
                fields,
                maximumNumberOfHits,
                boosts,
                type,
                sortBy,
                skip,
                take);

            return ScoredSearch(options);
        }
    }
}
