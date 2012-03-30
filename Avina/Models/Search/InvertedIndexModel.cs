namespace Avina.Models.Search
{
    using System.Collections.Generic;
    using MongoDB.Bson;

    public class InvertedIndexModel
    {
        public ObjectId Id { get; set; }

        /// <summary>
        /// Keyword
        /// </summary>
        public string kw { get; set; }

        /// <summary>
        /// Object Id's of SiteRecords that have the keyword
        /// </summary>
        public List<ObjectId> srIds { get; set; }
    }
}