using System.Threading.Tasks;
using CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces
{
    public interface ISearchableContextProvider<CTX> where CTX : IDbContext, new()
    {
        CTX Context { get; }
        int IndexCount { get; }

        void CreateIndex(bool optimize = false);
        void DeleteIndex();
        void Initialize(LuceneIndexerOptions indexerOptions, bool overrideIfExists);
        void Initialize(LuceneIndexerOptions indexerOptions, CTX context, bool overrideIfExists);
        int SaveChanges(bool index = true);
        Task<int> SaveChangesAsync(bool index = true);
        IScoredSearchResultCollection<ILuceneIndexable> ScoredSearch(SearchOptions options);
        IScoredSearchResultCollection<T> ScoredSearch<T>(SearchOptions options);
        ISearchResultCollection<ILuceneIndexable> Search(SearchOptions options);
        ISearchResultCollection<T> Search<T>(SearchOptions options);
    }
}