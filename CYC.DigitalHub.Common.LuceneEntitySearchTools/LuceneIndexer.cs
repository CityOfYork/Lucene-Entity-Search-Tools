using CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces;
using Lucene.Net.Analysis;
using Lucene.Net.Index;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools
{
    public class LuceneIndexer : ILuceneIndexer
    {
        // Static instances of directory and analyzer. Lucene will take care of file locking
        private static Directory directory;
        private static Analyzer analyzer;

        // Mamximum field length in number of terms/tokens. If null the will be set to unlimited
        IndexWriter.MaxFieldLength maxFieldLength;

        public LuceneIndexer(Directory directory, Analyzer analyzer, int? maximumFieldLength = null)
        {
            if(maximumFieldLength.HasValue)
            {
                maxFieldLength = new IndexWriter.MaxFieldLength(maximumFieldLength.Value);
            }
            else
            {
                maxFieldLength = IndexWriter.MaxFieldLength.UNLIMITED;
            }

            LuceneIndexer.directory = directory;
            LuceneIndexer.analyzer = analyzer;
        }

        /// <summary>
        /// Add an ILuceneIndexable entity to the index
        /// </summary>
        /// <param name="entity">The ILuceneIndexable entity to add</param>
        /// <param name="optimize">Whether to optimize the index afterwards</param>
        public void Add(ILuceneIndexable entity, bool optimize = false)
        {
            Update(new LuceneIndexChange(entity, LuceneIndexState.Added), optimize);
        }

        public void CreateIndex(IEnumerable<ILuceneIndexable> entities, bool recreate = true, bool optimize = true)
        {
            using (var writer = new IndexWriter(directory, analyzer, maxFieldLength))
            {
                // Clear out the index if we are recreating it
                if (recreate == true)
                {
                    writer.DeleteAll();
                    writer.Commit();
                }

                // add the entities to the index
                foreach(var entity in entities)
                {
                    // convert entity to document and add it to the index
                    writer.AddDocument(entity.ToDocument());
                }

                // if the optimize flag has been set, then optimize the index.
                //
                // NOTE: This is set true by default because you would normally want to optimize
                // after creating the index
                //
                if(optimize == true)
                {
                    writer.Optimize();
                }

                // flush all in memory buffered changes and write them
                writer.Flush(true, true, true);
            }
        }

        public void Delete(ILuceneIndexable entity, bool optimize = false)
        {
            Update(new LuceneIndexChange(entity, LuceneIndexState.Removed), optimize);
        }

        /// <summary>
        /// Delete all the documents from the index
        /// </summary>
        /// <param name="commit">Whether to commit all pending changes</param>
        public void DeleteAll(bool commit = true)
        {
            using (var writer = new IndexWriter(directory, analyzer, maxFieldLength))
            {
                writer.DeleteAll();
                if(commit == true) { writer.Commit(); }
            }
        }

        /// <summary>
        /// Update an ILuceneIndexable entity
        /// </summary>
        /// <param name="entity">The entity to update</param>
        /// <param name="optimize">Whether to optimize the index afterwards</param>
        public void Update(ILuceneIndexable entity, bool optimize = false)
        {
            Update(new LuceneIndexChange(entity, LuceneIndexState.Updated), optimize);
        }

        /// <summary>
        /// Update a single LuceneIndexChange
        /// </summary>
        /// <param name="change">The change to make</param>
        /// <param name="optimize">Whether to optimize afterwards</param>
        public void Update(LuceneIndexChange change, bool optimize = false)
        {
            LuceneIndexChangeset changeset = new LuceneIndexChangeset(change, optimize);
            Update(changeset);
        }

        public void Update(LuceneIndexChangeset changeset)
        {
            using (var writer = new IndexWriter(directory, analyzer, maxFieldLength))
            {
                foreach(var change in changeset.Entries)
                {
                    switch(change.State)
                    {
                        case LuceneIndexState.Added:
                            writer.AddDocument(change.Entity.ToDocument());
                            break;
                        case LuceneIndexState.Removed:
                            writer.DeleteDocuments(new Term("IndexId", change.Entity.IndexId.ToString()));
                            break;
                        case LuceneIndexState.Updated:
                            writer.UpdateDocument(new Term("IndexId", change.Entity.IndexId.ToString()), change.Entity.ToDocument());
                            break;
                        default:
                            break; // do nothing if the state doesn't involve changing the data
                    }
                }

                // only call the optimize if specified in the changeset
                // as this can be potentially quite an expensive task
                if(changeset.Optimize == true)
                {
                    writer.Optimize();
                }

                // flush the changes
                writer.Flush(true, true, changeset.HasDeletes);
            }
        }

        /// <summary>
        /// Returns the number of documents in the index
        /// </summary>
        /// <returns>int - the number of documents in the index</returns>
        public int Count()
        {
            if(!IndexReader.IndexExists(directory)) { return 0; }

            IndexReader reader = IndexReader.Open(directory, true);
            return reader.NumDocs();
        }
    }
}
