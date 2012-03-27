namespace Avina.Models
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
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
            if (Regex.IsMatch(model.Referrer, @"(http|https):\/\/avina") || 
                Regex.IsMatch(model.Referrer, @"(http|https):\/\/localhost")) return;

            if (!model.IsValid)
            {
                Debug.WriteLine("URL REJECTED (" + model.Url + ")");
                return;
            }

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
                    referrerUrl = model.Referrer,
                    textPreview = model.FirstP
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

        public void Remove(string url)
        {
            this.Database.GetCollection<SiteRecord>("UrlList")
                                     .Remove(Query.EQ("url", url));
        }

        public void IncrementClick(string url)
        {
            var record = this.Database.GetCollection<SiteRecord>("UrlList")
                             .Find(Query.EQ("url", url))
                             .Single();
            record.hits += 1;
            Debug.WriteLine(string.Format("hits:{0}\tsubmits:{1}", record.hits, record.duplicates));
            this.Database.GetCollection<SiteRecord>("UrlList")
                         .Save(record);
        }

        /// <summary>
        /// Have a look through the database and apply all of the
        /// url filters retroactively
        /// </summary>
        public void ApplyFiltersRetro()
        {
            foreach (var record in this.GetAll().ToList())
            {
                foreach (var regex in SubmissionModel.RegexFilters)
                {
                    if (Regex.IsMatch(record.url, regex))
                    {
                        this.Database.GetCollection<SiteRecord>("UrlList")
                                     .Remove(Query.EQ("url", record.url));
                    }
                }
            }
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

        public IEnumerable<SiteRecord> Search(string sSearch)
        {
            if (sSearch.IsNullEmptyOrWhitespace())
                return new List<SiteRecord>();
            
            return InvertedIndex.ApplyTerms(sSearch.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList());
        }

        public IEnumerable<SiteRecord> DataTableQuery(DataTableParameterModel model, out long nRecords)
        {
            model = model ?? new DataTableParameterModel();

            IEnumerable<SiteRecord> searchResults;
            if (model.sSearch.IsNullEmptyOrWhitespace())
            {
                // Search string is no good, so it's not a query. Retrieve popular shit
                searchResults = this.Database.GetCollection<SiteRecord>("UrlList")
                                    .FindAll()
                                    .OrderByDescending(r => r.hits)
                                    .ThenByDescending(r => r.duplicates);
            }
            else
            {
                var keywords = model.sSearch.Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries).ToList();
                bool clickSort, dupSort;
                this.ParseSorting(model, out clickSort, out dupSort);
                searchResults  = InvertedIndex.ApplyTerms(keywords, dupSort, clickSort);
            }
            nRecords = searchResults.Count();
            return this.PagedQuery(searchResults, model);
        }

        #region DataTable Backend Methods

        private void ParseSorting(DataTableParameterModel model, out bool clickSort, out bool dupSort)
        {
            clickSort = false;
            dupSort = false;

            if (model.sSortDir == null)
            {
                return;
            }

            // This totally needs refactoring
            for (int i = 0; i < model.iSortingCols; ++i)
            {
                var ascending = string.Equals("asc", model.sSortDir[i], StringComparison.OrdinalIgnoreCase);
                int sortCol = model.iSortCol[i];
                
                if (sortCol == 1) clickSort = ascending;
                if (sortCol == 2) dupSort = ascending;
            }
        }

        private IEnumerable<SiteRecord> PagedQuery(IEnumerable<SiteRecord> records, DataTableParameterModel model)
        {
            return records.Skip(model.iDisplayStart)
                          .Take(model.iDisplayLength);
        }

        #endregion
    }
}