using Lucene.Net.Documents;
using System;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces
{
    /// <summary>
    /// Interface to enable indexing an entity
    /// Id - The entities own Id
    /// IndexId - Guid that can be used by Lucene to identify the entity across multiple object types
    /// Type - the entities underlying .Net type 
    /// NOTE: If using entity framework don't assume that 'this.GetType()' will work. It may not!
    /// You will need to use 'return typeof(myType);' explicitly, because entity framework may wrap
    /// your poco up in a dynamic proxy object
    /// AsLuceneDocument - Implementation that converts the entity into a Lucene Document.
    /// </summary>
    public interface ILuceneIndexable
    {
        int Id { get; set; }
        Guid IndexId { get; set; }
        Type Type { get; }
        Document ToDocument();
    }
}
