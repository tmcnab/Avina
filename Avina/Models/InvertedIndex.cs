namespace Avina.Models
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using Avina.Extensions;
    using Avina.Models.Search;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;

    public static class InvertedIndex
    {
        static InvertedIndex()
        {
            Database = MongoDatabase.Create(ConfigurationManager.AppSettings.Get("MONGOLAB_URI"));
            Rebuilding = false;
            NProcessedQueries = 0;
            TTotalQueryTime = 0;
        }

        #region Properties

        private static MongoDatabase Database { get; set; }

        static ulong NProcessedQueries { get; set; }

        static double TTotalQueryTime { get; set; }

        public static double TAverageQueryTime
        {
            get
            {
                return TTotalQueryTime / NProcessedQueries == 0 ? 1 : NProcessedQueries;
            }
        }

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

            try
            {
                if (andPurge) Purge();

                var invertedIndex = Database.GetCollection<InvertedIndexModel>("InvertedIndex");
                var records = Database.GetCollection<SiteRecord>("UrlList").FindAll();

                ProcessingTotal = records.Count();
                ProcessingCurrent = 0;

                // Iterate over EVERY. SINGLE. ITEM. and apply it to the invidx
                foreach (var record in records)
                {
                    // Split the record title into tokens, remove shit characters
                    var keywords = ParseKeywords(record.title ?? string.Empty);

                    // Iterate over every keyword
                    for (int i = 0; i < keywords.Length; i++)
                    {
                        var iidxItem = invertedIndex.Find(Query.EQ("kw", keywords[i])).SingleOrDefault();
                        iidxItem = iidxItem ?? new InvertedIndexModel()
                        {
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
            }
            catch (Exception ex)
            {
                ex.SendToACP();
            }
            finally
            {
                Rebuilding = false;
            }
        }

        /// <summary>
        /// The heart of the search engine - the searching part
        /// </summary>
        /// <param name="terms">The strings to apply against the inverted index</param>
        /// <param name="dupSort">default or false: descending ; true: ascending</param>
        /// <param name="hitSort">default or false: descending ; true: ascending</param>
        /// <returns>A sequence of items (in a specific order) that are the results of the search</returns>
        public static IEnumerable<SiteRecord> ApplyTerms(List<string> terms, bool dupSort = false, bool hitSort = false)
        {
            // Guard for empty lists
            if (terms.Count < 1) yield break;

            // Perf Vars
            NProcessedQueries += 1;
            var startTime = DateTime.UtcNow;

            // DB Connection to the Inverted Index (iidx)
            var iidx = Database.GetCollection<InvertedIndexModel>("InvertedIndex");

            // For every keyword, add the Ids associated with the keyword to a pool (iidxReduceResults)
            // TODO: this could probably be turned into a MongoDB batch query sometime down the line
            var iidxReduceResults = new List<ObjectId>();
            terms.ForEach((keyword) =>
            {
                var indexItem = iidx.FindOne(Query.EQ("kw", keyword));
                indexItem = indexItem ?? new InvertedIndexModel()
                {
                    srIds = new List<ObjectId>()
                };
                iidxReduceResults.AddRange(indexItem.srIds);
            });


            // A fucking retarded way to get my column sorting useful for DataTables. Talk about 
            // putting the cart before the horse!
            var resultsIndex = Database.GetCollection<SiteRecord>("UrlList");
            IEnumerable<ObjectId> iidxFinalSorted;
            if (dupSort)
            {
                if (hitSort)
                {
                    iidxFinalSorted = iidxReduceResults.OrderByDescending(p => iidxReduceResults.Where(q => q == p).Count())
                                                   .ThenBy(p => resultsIndex.FindOneById(p).duplicates)
                                                   .ThenBy(p => resultsIndex.FindOneById(p).hits)
                                                   .Distinct();
                }
                else
                {
                    iidxFinalSorted = iidxReduceResults.OrderByDescending(p => iidxReduceResults.Where(q => q == p).Count())
                                                   .ThenBy(p => resultsIndex.FindOneById(p).duplicates)
                                                   .ThenByDescending(p => resultsIndex.FindOneById(p).hits)
                                                   .Distinct();
                }
            }
            else
            {
                if (hitSort)
                {
                    iidxFinalSorted = iidxReduceResults.OrderByDescending(p => iidxReduceResults.Where(q => q == p).Count())
                                                   .ThenByDescending(p => resultsIndex.FindOneById(p).duplicates)
                                                   .ThenBy(p => resultsIndex.FindOneById(p).hits)
                                                   .Distinct();
                }
                else
                {
                    iidxFinalSorted = iidxReduceResults.OrderByDescending(p => iidxReduceResults.Where(q => q == p).Count())
                                                       .ThenByDescending(p => resultsIndex.FindOneById(p).duplicates)
                                                       .ThenByDescending(p => resultsIndex.FindOneById(p).hits)
                                                       .Distinct();
                }
            }

            // Transmute the sorted index into SiteRecords yielded as a result
            foreach (var id in iidxFinalSorted)
	        {
                yield return resultsIndex.FindOneById(id);
	        }

            // Finalize the Perf stuff
            TTotalQueryTime += (DateTime.UtcNow - startTime).TotalMilliseconds;

            #if DEBUG
            Debug.WriteLine(string.Format("Executed Query in {0}ms", (DateTime.UtcNow - startTime).TotalMilliseconds));
            #endif
        }

        public static void Add(SiteRecord record)
        {
            // TODO: implement
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

        private static char[] TrimChars = new char[] { ' ', '{', '}', '(', ')', ':', '`'};

        #endregion
    }
}