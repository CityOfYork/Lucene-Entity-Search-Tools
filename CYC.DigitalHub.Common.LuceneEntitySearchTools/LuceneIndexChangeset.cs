using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools
{
    /// <summary>
    /// An IList of LuceneIndexChange objects, with some additional helper properties
    /// </summary>
    public class LuceneIndexChangeset
    {
        public IList<LuceneIndexChange> Entries { get; set; }

        public bool Optimize { get; set; }
        private bool EntriesHaveState(LuceneIndexState state)
        {
            return Entries.Any(x => x.State == state);
        }

        public bool HasAdds { get { return EntriesHaveState(LuceneIndexState.Added); } }
        public bool HasUpdates { get { return EntriesHaveState(LuceneIndexState.Updated); } }
        public bool HasDeletes { get { return EntriesHaveState(LuceneIndexState.Removed); } }

        public bool HasChanges
        {
            get
            {
                return Entries.Any() && (HasAdds || HasUpdates || HasDeletes);
            }
        }

        public LuceneIndexChangeset()
        {
            Entries = new List<LuceneIndexChange>();
        }

        public LuceneIndexChangeset(LuceneIndexChange change, bool optimize = false)
        {
            Entries = new List<LuceneIndexChange>();
            Entries.Add(change);
            Optimize = optimize;
        }

        public LuceneIndexChangeset(bool optimize)
        {
            Entries = new List<LuceneIndexChange>();
            Optimize = optimize;
        }
    }
}
