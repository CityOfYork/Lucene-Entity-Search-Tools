using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces
{
    public interface ILuceneIndexSearcher
    {
        ILuceneSearchResultCollection ScoredSearch(SearchOptions options);
        ILuceneSearchResultCollection ScoredSearch(
            string searchText,
            string fields,
            int maximumNumberOfHits,
            Dictionary<string, float> boosts,
            Type type,
            string sortBy,
            int? skip,
            int? take);
    }
}
