namespace Avina.Models
{
    using System;
    using System.Collections.Concurrent;
    using System.Configuration;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Avina.Extensions;
    using Avina.Models.Requests;
    using HtmlAgilityPack;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;

    /// <summary>
    /// The forward index stores each and every search result.
    /// </summary>
    public static class ForwardIndex
    {
        static ConcurrentQueue<SiteSubmission> ProcessingQueue = new ConcurrentQueue<SiteSubmission>();

        //public static HashSet<string> UrlFilters = new HashSet<string>();
        public static string[] UrlFilters = new string[] {
            @"(http|https):\/\/avina",
            @"(http|https):\/\/localhost",
            @"(http|https):\/\/[w.]+google.[A-Za-z.]+\/search",
            @"(http|https):\/\/[w.]+google.[A-Za-z.]+\/webhp",
            @"(http|https):\/\/[w.]+google.[A-Za-z.]+\/imgres",
            @"(http|https):\/\/[w.]+facebook.com\/plugins\/like",
            @"(http|https):\/\/[w.]+bing.com\/search",
            @"(http|https):\/\/[A-Za-z].yahoo.com\/search",
            @"(http|https):\/\/platform.twitter",
            @"(http|https):\/\/duckduckgo.com\/\?q=",
            @"(http|https):\/\/[A-Za-z0.9\-\.]+\/search",
            @"(http|https):\/\/stackoverflow.com\/questions\/tagged",
            @"about:blank"
        };

        static bool IsProcessing { get; set; }

        public static void Start()
        {
            ForwardIndex.IsProcessing = true;
            Task.Factory.StartNew(() => ForwardIndex.Process(), TaskCreationOptions.LongRunning);
        }

        public static void Stop()
        {
            ForwardIndex.IsProcessing = false;
        }

        public static void Add(SiteSubmission model)
        {
            #if DEBUG
            Debug.WriteLine(string.Format("ForwardIndex::Add({0})", model.Url));
            #endif
            ForwardIndex.ProcessingQueue.Enqueue(model);
        }

        private static void Process()
        {
            while (ForwardIndex.IsProcessing)
            {
                // Check that there's something to process.
                if (ForwardIndex.ProcessingQueue.IsEmpty)
                {
                    Thread.Sleep(250);
                }
                else
                {
                    // If the model can't be dequeued, or the model is invalid, it'll get disposed of
                    SiteSubmission model;
                    if (ForwardIndex.ProcessingQueue.TryDequeue(out model) && model.IsValid)
                    {
                        #if DEBUG
                        Debug.WriteLine(string.Format("ForwardIndex::Process({0})", model.ToString()));
                        #endif

                        // Grab our Database connection
                        var Database = MongoDatabase.Create(ConfigurationManager.AppSettings.Get("MONGOLAB_URI"));

                        // Try and see if we've already found this url before
                        var record = Database.GetCollection<SiteRecord>("UrlList")
                                             .FindOne(Query.EQ("url", model.Url));

                        // If it wasn't found, create one (otherwise update it)
                        if (record == null)
                        {
                            // go grab the necessary information from the website here through ForwardIndex.Crawl
                            record = ForwardIndex.Crawl(new SiteRecord()
                            {
                                duplicates = 0,
                                hits = 0,
                                submitter = model.SubmitterIP,
                                referrerUrl = model.Referrer,
                                url = model.Url,
                                submitted = model.Submitted
                            });
                            if (record == null) continue;
                        }
                        else
                        {
                            // Update the duplicate count, check if the record is older than 24 hours and re-crawl
                            // if that is the case
                            record.duplicates += 1;
                            if ((DateTime.UtcNow - record.submitted).TotalHours > 24)
                            {
                                record = ForwardIndex.Crawl(record);
                            }
                            if (record == null) continue;
                        }

                        // Save it to the database
                        Database.GetCollection<SiteRecord>("UrlList").Save(record);

                        // Add the referrer to the Database too
                        if (!model.Referrer.IsNullEmptyOrWhitespace())
                        {
                            ProcessingQueue.Enqueue(new SiteSubmission()
                            {
                                Referrer = string.Empty,
                                Submitted = model.Submitted,
                                SubmitterIP = model.SubmitterIP,
                                Url = model.Referrer
                            });
                        }
                    }
                }
            }
        }

        private static SiteRecord Crawl(SiteRecord record)
        {
            const int MIN_P_LENGTH = 15;
            const int MAX_P_LENGTH = 157;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(record.url);
                var doc = new HtmlDocument();
                doc.Load(request.GetResponse().GetResponseStream());

                record.title = doc.DocumentNode.SelectSingleNode("//title").InnerText;
                if (record.title.IsNullEmptyOrWhitespace())
                {
                    return null;
                }

                foreach (var pElem in doc.DocumentNode.SelectNodes("//p"))
                {
                    record.textPreview = pElem.InnerText.Trim();
                    if (record.textPreview.Length < MIN_P_LENGTH)
                    {
                        continue;
                    }
                    else
                    {
                        if (record.textPreview.Length > MAX_P_LENGTH + 3)
                        {
                            record.textPreview = record.textPreview.Substring(0, MAX_P_LENGTH) + "...";
                        }
                        break;
                    }
                }

                if (record.textPreview.Length < MIN_P_LENGTH)
                {
                    return null;
                }
            }
            catch { }
            
            return record;
        }
    }
}