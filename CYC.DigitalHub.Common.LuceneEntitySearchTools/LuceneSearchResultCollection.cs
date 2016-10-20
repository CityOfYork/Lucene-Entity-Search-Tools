using CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools
{
    public class LuceneSearchResultCollection : ILuceneSearchResultCollection
    {
        public IList<ILuceneSearchResult> Results { get; set; } = new List<ILuceneSearchResult>();

        public long TimeTaken { get; set; }

        public int TotalHits { get; set; }
    }
}
