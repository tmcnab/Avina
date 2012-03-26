namespace Avina.Models
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;

    public static class InvertedIndex
    {
        private static MongoDatabase Database { get; set; }

        public static bool Rebuilding { get; private set; }

        public static long Current { get; private set; }

        public static long Total { get; private set; }

        static InvertedIndex()
        {
            Database = MongoDatabase.Create(ConfigurationManager.AppSettings.Get("MONGOLAB_URI"));
            Rebuilding = false;
        }

        public static List<InvertedIndexModel> GetIndex()
        {
            return Database.GetCollection<InvertedIndexModel>("InvertedIndex")
                           .FindAll()
                           .OrderByDescending(d => d.srIds.Count())
                           .ToList();
        }

        public static void Rebuild(bool andPurge = false)
        {
            Rebuilding = true;

            if (andPurge) Purge();

            var invertedIndex = Database.GetCollection<InvertedIndexModel>("InvertedIndex");
            var records = Database.GetCollection<SiteRecord>("UrlList").FindAll();

            Total = records.Count();
            Current = 0;

            // Iterate over EVERY. SINGLE. ITEM. and apply it to the invidx
            foreach (var record in records)
            {
                // Split the record title into tokens, remove shit characters
                var keywords = ParseKeywords(record.title);

                // Iterate over every keyword
                for (int i = 0; i < keywords.Length; i++)
                {
                    var iidxItem = invertedIndex.Find(Query.EQ("kw", keywords[i])).SingleOrDefault();
                    iidxItem = iidxItem ?? new InvertedIndexModel() {
                        kw = keywords[i],
                        srIds = new List<ObjectId>(),
                        Id = ObjectId.GenerateNewId()
                    };
                    Debug.WriteLine(keywords[i]);
                    
                    iidxItem.srIds.Add(record.Id);
                    invertedIndex.Save<InvertedIndexModel>(iidxItem);
                }

                // Update the number processed
                Current += 1;
            }

            Rebuilding = false;
        }

        private static void Purge()
        {
            Database.GetCollection<InvertedIndexModel>("InvertedIndex").Drop();
        }

        private static string[] ParseKeywords(string s)
        {
            return (new string(s.ToArray()
                                .Where(c => !char.IsSymbol(c)).ToArray()))
                        .ToLowerInvariant()
                        .Replace("-", string.Empty)
                        .Replace("...", string.Empty)
                        .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(t => t.Length > 2)
                        .ToList()
                        .ToArray();
        }

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
}