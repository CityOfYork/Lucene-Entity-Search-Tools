using CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools
{
    public class ScoredSearchResultCollection<T> : IScoredSearchResultCollection<T>
    {
        public IList<IScoredSearchResult<T>> Results { get; set; } = new List<IScoredSearchResult<T>>();

        public long TimeTaken { get; set; }

        public int TotalHits { get; set; }
    }
}
