using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools.Tests.Models
{
    public class User : LuceneIndexableEntityBase
    {
        [LuceneIndexable]
        public string FirstName { get; set; }

        [LuceneIndexable]
        public string Surname { get; set; }

        [LuceneIndexable]
        public string Email { get; set; }

        [LuceneIndexable]
        public string JobTitle { get; set; }

        public override Type Type { get { return typeof(User); } }
    }
}
