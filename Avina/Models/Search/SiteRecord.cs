namespace Avina.Models.Search
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using MongoDB;
    using MongoDB.Driver;
    using MongoDB.Bson;

    public class SiteRecord
    {
        public ObjectId Id { get; set; }

        public string url { get; set; }

        public string referrerUrl { get; set; }

        public string title { get; set; }

        public long duplicates { get; set; }

        public long hits { get; set; }

        public string submitter { get; set; }

        public DateTime submitted { get; set; }

        public string textPreview { get; set; }

        public class DistinctItemComparer : IEqualityComparer<SiteRecord>
        {
            public bool Equals(SiteRecord x, SiteRecord y)
            {
                return (x.url == y.url) && (x.title == y.title);
            }

            public int GetHashCode(SiteRecord obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}