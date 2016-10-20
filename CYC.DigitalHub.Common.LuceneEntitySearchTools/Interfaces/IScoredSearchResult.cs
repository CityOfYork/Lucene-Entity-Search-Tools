using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces
{
    public interface IScoredSearchResult<T>
    {
        float Score { get; set; }
        T Entity { get; set; }
    }
}
