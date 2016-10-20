using CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools
{
    public class SearchResultCollection<T> : ISearchResultCollection<T>
    {
        public IList<T> Results { get; set; }

        public long TimeTaken { get; set; }

        public int TotalHits { get; set; }

        public SearchResultCollection()
        {
            Results = new List<T>();
        }
    }
}
