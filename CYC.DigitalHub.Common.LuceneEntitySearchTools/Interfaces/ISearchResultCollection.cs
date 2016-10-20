using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces
{
    public interface ISearchResultCollection<T>
    {
        int TotalHits { get; set; }
        long TimeTaken { get; set; }
        IList<T> Results { get; set; }
    }
}
