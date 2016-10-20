using CYC.DigitalHub.Common.LuceneEntitySearchTools.Helpers;
using CYC.DigitalHub.Common.LuceneEntitySearchTools.Interfaces;
using Lucene.Net.Documents;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools
{
    public abstract class LuceneIndexableEntityBase : ILuceneIndexable
    {
        [LuceneIndexable(Name = "Id", Store = Field.Store.YES, Index = Field.Index.NOT_ANALYZED)]
        public int Id { get; set; }

        [LuceneIndexable(Name = "IndexId", Store = Field.Store.YES, Index = Field.Index.NOT_ANALYZED)]
        public Guid IndexId { get; set; }

        public abstract Type Type { get; }

        public virtual Document ToDocument()
        {
            // The Lucene Document
            Document doc = new Document();

            // Get the class type and it's PropertyInfo objects
            PropertyInfo[] classProperties = Type.GetProperties();

            // add the class type to the document
            doc.Add(new Field("Type", Type.AssemblyQualifiedName, Field.Store.YES, Field.Index.NOT_ANALYZED));

            // go through the class properties and get the attributes
            foreach(PropertyInfo propertyInfo in classProperties)
            {
                // the value of the property
                object propertyValue = propertyInfo.GetValue(this);
                if(propertyValue != null)
                {
                    // get the LuceneIndexableAttributes, if any
                    IEnumerable<LuceneIndexableAttribute> attrs = propertyInfo.GetCustomAttributes<LuceneIndexableAttribute>();

                    // interate the attributes - NOTE: There may be more than one per property
                    foreach(LuceneIndexableAttribute attr in attrs)
                    {
                        // the name to be used in the index
                        string name = (!string.IsNullOrEmpty(attr.Name) ? attr.Name : propertyInfo.Name);

                        // the value, converted from Html if neccessary
                        string value = (attr.IsHtml)
                            ? StringHelpers.RemoveUnwantedTags(propertyValue.ToString())
                            : propertyValue.ToString();

                        // Add the property to the document
                        doc.Add(new Field(name, value, attr.Store, attr.Index));
                    }

                }
            }

            // return the document, ready for indexing
            return doc;
        }
    }
}
