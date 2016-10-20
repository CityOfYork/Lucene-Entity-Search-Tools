using CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces;
using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools
{
    public class LuceneSearchResult : ILuceneSearchResult
    {
        public float Score { get; set; }
        public Document Document { get; set; }
    }


}
