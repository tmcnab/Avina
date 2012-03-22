namespace Avina.Models
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Avina.Extensions;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;

    public class Repository
    {
        public MongoDatabase Database { get; private set; }

        public Repository()
        {
            this.Database = MongoDatabase.Create(ConfigurationManager.AppSettings.Get("MONGOLAB_URI"));
        }

        public void Add(SubmissionModel model)
        {
            if (!model.IsValid) return;

            // Try and see if we've already found this url before
            var record = this.Database.GetCollection<SiteRecord>("UrlList")
                             .FindAll()
                             .SingleOrDefault(u => u.url == model.Url);

            // If it wasn't found, create one (otherwise update it)
            if (record == null)
            {
                record = new SiteRecord()
                {
                    duplicates = 0,
                    hits = 0,
                    submitter = model.HostIP,
                    title = model.Title,
                    url = model.Url,
                    submitted = model.When,
                    referrerUrl = model.Referrer
                };
            }
            else
            {
                record.title = model.Title;
                record.duplicates += 1;
                if (record.referrerUrl.IsNullEmptyOrWhitespace() && !model.Referrer.IsNullEmptyOrWhitespace())
                {
                    record.referrerUrl = model.Referrer;
                }
            }

            // Save it to the database
            this.Database.GetCollection<SiteRecord>("UrlList").Save(record);
            
            // Save the referrer as well (maybe for future functionality)
            this.Add(new SubmissionModel(model.Referrer, model.HostIP));
        }

        public void IncrementClick(string url)
        {
            var record = this.Database.GetCollection<SiteRecord>("UrlList")
                             .Find(Query.EQ("url", url))
                             .Single();
            record.hits += 1;
            this.Database.GetCollection<SiteRecord>("UrlList")
                         .Save(record);
        }

        /// <summary>
        /// Returns every single URL Submission
        /// </summary>
        public IEnumerable<SiteRecord> GetAll()
        {
            return this.Database.GetCollection<SiteRecord>("UrlList")
                       .FindAll()
                       .ToList();
        }
    }
}