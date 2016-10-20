using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools
{
    /// <summary>
    /// Enum for specifying the change state in a LuceneIndexChange
    /// </summary>
    public enum LuceneIndexState
    {
        Added, Removed, Updated, Unchanged, NotSet
    }
}
