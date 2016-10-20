using CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools
{
    public class ScoredSearchResult<T>: IScoredSearchResult<T>
    {
        public float Score { get; set; }
        public T Entity { get; set; }
    }
}
