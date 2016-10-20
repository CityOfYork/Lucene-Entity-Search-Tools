using Lucene.Net.Documents;
using System;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools
{
    /// <summary>
    /// Attribute to allow a property to be indexed by LuceneIndexableEntityBase class
    /// 
    /// Name = the name used to identify the property in the index
    /// Index = options for whether the property should be indexed or not
    /// Store = options for whether the property should be stored or not
    /// IsHtml = specify if the data is html ( tags will then be stripped )
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LuceneIndexableAttribute : Attribute
    {
        private string name;
        private Field.Index index;
        private Field.Store store;
        private bool isHtml;

        // Default Constructor
        public LuceneIndexableAttribute()
        {
            index = Field.Index.ANALYZED;
            store = Field.Store.YES;
            isHtml = false;
        }

        // Attribute public properties
        public string Name { get { return name; } set { name = value; } }
        public Field.Index Index { get { return index; } set { index = value; } }
        public Field.Store Store { get { return store; } set { store = value; } }
        public bool IsHtml { get { return isHtml; } set { isHtml = value; } }
    }
}
