using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces
{
    public interface IScoredSearchResultCollection<T>
    {
        int TotalHits { get; set; }
        long TimeTaken { get; set; }
        IList<IScoredSearchResult<T>> Results { get; set; }
    }

    public interface IScoredSearchResultCollection
    {
        int TotalHits { get; }
        long TimeTaken { get; set; }
        IList<IScoredSearchResult<ILuceneIndexable>> Results { get; set; }
    }
}
