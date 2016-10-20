using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests.Models
{
    public class City : LuceneIndexableEntityBase
    {
        [LuceneIndexable]
        public string Name { get; set; }

        public string Code { get; set; }

        [LuceneIndexable]
        public string Country { get; set; }

        public override Type Type { get { return typeof(City); } }
    }
}
