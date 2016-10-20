using CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools
{
    /// <summary>
    /// Represents a single change within the Lucene Index
    /// </summary>
    public class LuceneIndexChange
    {
        public ILuceneIndexable Entity { get; set; }
        public LuceneIndexState State { get; set; }

        public LuceneIndexChange(ILuceneIndexable entity)
        {
            Entity = entity;
            State = LuceneIndexState.NotSet;
        }

        public LuceneIndexChange(ILuceneIndexable entity, LuceneIndexState state)
        {
            Entity = entity;
            State = state;
        }
    }
}
