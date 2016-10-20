using System;
using System.Collections.Generic;
using System.Linq;
using CYC.DigitalHub.Common.LuceneEntitySearchTools.Helpers;

namespace CYC.DigitalHub.Common.LuceneEntitySearchTools
{
    public class SearchOptions
    {
        public string SearchText { get; set; }

        public List<string> Fields { get; set; }

        public int MaximumNumberOfHits { get; set; }

        private Dictionary<string, float> boosts;

        public Dictionary<string, float> Boosts
        {
            get
            {
                // add and boosts in that are missing
                foreach (var field in Fields)
                {
                    if (!boosts.Any(x => x.Key.ToUpper() == field.ToUpper()))
                    {
                        boosts.Add(field, 1.0f);
                    }
                }

                return boosts;
            }
        }

        public void ClearBoosts()
        {
            boosts.Clear();
        }

        public void SetBoost(string key, float value)
        {
            boosts[key] = value;
        }

        public void SetBoosts(Dictionary<string, float> boosts)
        {
            this.boosts = boosts;
        }

        public List<string> OrderBy { get; set; }

        public int? Skip { get; set; }
        public int? Take { get; set; }

        public Type Type { get; set; }

        public SearchOptions()
        {
            Fields = new List<string>();
            OrderBy = new List<string>();
            MaximumNumberOfHits = 1000;
            boosts = new Dictionary<string, float>();
        }

        public SearchOptions(string searchText, 
            string fields,
            int maximumNumberOfHits = 1000,
            Dictionary<string, float> boosts = null,
            Type type = null,
            string orderBy = null,
            int? skip = null,
            int? take = null
            )
        {
            SearchText = searchText;
            MaximumNumberOfHits = maximumNumberOfHits;

            this.boosts = boosts;
            if(this.boosts == null) { this.boosts = new Dictionary<string, float>(); }

            Type = type;

            Fields = new List<string>();
            OrderBy = new List<string>();

            // add the fields
            if(!string.IsNullOrEmpty(fields))
            {
                fields = fields.RemoveCharacters(" ");
                Fields.AddRange(fields.Split(',').ToList());
            }

            // add the OrderBy's
            if(!string.IsNullOrEmpty(orderBy))
            {
                orderBy = orderBy.RemoveCharacters(" ");
                OrderBy.AddRange(orderBy.Split(',').ToList());
            }
        }
    }
}
