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
        static InvertedIndex()
        {
            Database = MongoDatabase.Create(ConfigurationManager.AppSettings.Get("MONGOLAB_URI"));
            Rebuilding = false;
        }

        #region Properties

        private static MongoDatabase Database { get; set; }

        public static bool Rebuilding { get; private set; }

        public static long ProcessingCurrent { get; private set; }

        public static long ProcessingTotal { get; private set; }

        public static long TotalEntries
        {
            get
            {
                return Database.GetCollection<InvertedIndexModel>("InvertedIndex").FindAll().Count();
            }
        }

        #endregion

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

            ProcessingTotal = records.Count();
            ProcessingCurrent = 0;

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
                ProcessingCurrent += 1;
            }

            Rebuilding = false;
        }

        public static IEnumerable<SiteRecord> ApplyTerms(List<string> terms)
        {
            
            var iidx = Database.GetCollection<InvertedIndexModel>("InvertedIndex").FindAll();
            var iidxReduceResults = new List<ObjectId>();

            // For every keyword, add the ObjectId's to a pool
            terms.ForEach((keyword) =>
            {
                iidxReduceResults.AddRange(iidx.SingleOrDefault(a => a.kw == keyword).srIds ?? new List<ObjectId>());
            });

            #if DEBUG
            foreach (var item in iidxReduceResults)
            {
                Debug.WriteLine(string.Format("iidx1\t{0}", item));
            }
            #endif

            if (terms.Count > 1)
            {
                iidxReduceResults.GroupBy(g => g)
                                 .Where(g => g.Count() > 1)
                                 .Select(g => g.Key)
                                 .ToList();
            }
            else
            {
                iidxReduceResults.GroupBy(g => g)
                                 //.OrderByDescending(g => iidxReduceResults.Where(p => p == g.Key).Count())
                                 .Select(g => g.Key)
                                 .ToList();
            }
            #if DEBUG
            foreach (var item in iidxReduceResults)
            {
                Debug.WriteLine(string.Format("iidx2\t{0}", item));
            }
            #endif

            var resultsIndex = Database.GetCollection<SiteRecord>("UrlList");
            foreach (var id in iidxReduceResults.OrderByDescending(p => iidxReduceResults.Where(q => q == p).Count()).Distinct())
	        {
                yield return resultsIndex.FindOneById(id);
	        }
        }

        #region Backend Methods

        private static void Purge()
        {
            Database.GetCollection<InvertedIndexModel>("InvertedIndex").Drop();
        }

        private static string[] ParseKeywords(string s)
        {
            return (new string(s.ToArray()
                                .Where(c => !char.IsSymbol(c)).ToArray()))
                        .Trim()
                        .Trim(TrimChars)
                        .ToLowerInvariant()
                        .Replace("-", string.Empty)
                        .Replace("...", string.Empty)
                        .Split(new char[] { ' ' , ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(t => t.Length > 2)
                        .ToArray();
        }

        private static char[] TrimChars = new char[] { ' ', '{', '(', ':', '`'};

        #endregion

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