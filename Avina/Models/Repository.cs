﻿namespace Avina.Models
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using Avina.Extensions;
    using MongoDB.Driver;
    using MongoDB.Driver.Builders;
    using System.Text;
    using System.Text.RegularExpressions;

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
        /// Returns every single URL Submission
        /// </summary>
        public IEnumerable<SiteRecord> GetAll()
        {
            return this.Database.GetCollection<SiteRecord>("UrlList")
                       .FindAll()
                       .ToList();
        }

        public object DataTableQuery(DataTableParameterModel model)
        {
            model = model ?? new DataTableParameterModel();

            var records = from r in this.Database.GetCollection<SiteRecord>("UrlList").FindAll().AsQueryable()
                          select r;
                          
            if (!model.sSearch.IsNullEmptyOrWhitespace())
            {
                records = this.SearchQuery(records, model);
            }

            var nRecords = records.Count();

            records = this.SortQuery(records, model);
            records = this.PagedQuery(records, model);
            records = records.Take(500);

            var aaData = new List<object>();
            foreach (var item in records)
            {
                aaData.Add(new object[] {
                    new[] { item.url, item.title, item.textPreview ?? string.Empty },
                    item.hits,
                    item.duplicates
                });
            }

            return new
            {
                sEcho = model.sEcho,
                iTotalRecords = nRecords,
                iTotalDisplayRecords = nRecords,
                aaData = aaData.ToArray()
            };
        }

        #region DataTable Backend Methods

        private IQueryable<SiteRecord> SearchQuery(IQueryable<SiteRecord> records, DataTableParameterModel parameters)
        {
            // Split keywords into tokens
            var keywords = parameters.sSearch.Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries);

            foreach (var item in keywords)
            {
                if (!item.StartsWith("-")){
                    records = records.Where(k => k.title.ToLowerInvariant().Contains(item.ToLowerInvariant()));
                }
            }
            foreach (var item in keywords)
            {
                if (item.StartsWith("-"))
                {
                    records = records.Where(k => !(k.title.ToLowerInvariant().Contains(item.TrimStart('-').ToLowerInvariant())));
                }
            }

            return records;
        }

        private IOrderedQueryable<SiteRecord> SortQuery(IQueryable<SiteRecord> records, DataTableParameterModel parameters)
        {
            var orderedQuery = (IOrderedQueryable<SiteRecord>)records;

            if (records.Count() > 1 && !parameters.sSearch.IsNullEmptyOrWhitespace())
            {
                var keywords = parameters.sSearch.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var table = new Dictionary<string,long>();

                foreach (var item in records)
                {
                    long n = 0;
                    foreach (var keyword in keywords)
                    {
                        if (!keyword.StartsWith("-"))
                        {
                            if (item.title.ToLowerInvariant().Contains(keyword.ToLowerInvariant())) n += 1;
                        }
                    }
                    table.Add(item.url, n);
                }

                var sortedTable = table.OrderByDescending(d => d.Value);

                var sortedRecords = new List<SiteRecord>();
                foreach (var kvp in table.OrderByDescending(d => d.Value))
                {
                    Debug.WriteLine(string.Format("{0}:{1}", kvp.Value, kvp.Key));
                    sortedRecords.Add(orderedQuery.Single(u => u.url == kvp.Key));
                }

                return (IOrderedQueryable<SiteRecord>)sortedRecords.AsQueryable<SiteRecord>();
            }



            if (parameters.sSortDir == null) return orderedQuery;

            for (int i = 0; i < parameters.iSortingCols; ++i)
            {
                var ascending = string.Equals("asc", parameters.sSortDir[i], StringComparison.OrdinalIgnoreCase);
                int sortCol = parameters.iSortCol[i];

                Expression<Func<SiteRecord, long>> orderByExpression = GetOrderByExpression(sortCol);

                if (ascending)
                {
                    orderedQuery = (i == 0)
                        ? orderedQuery.OrderBy(orderByExpression)
                        : orderedQuery.ThenBy(orderByExpression);
                }
                else
                {
                    orderedQuery = (i == 0)
                        ? orderedQuery.OrderByDescending(orderByExpression)
                        : orderedQuery.ThenByDescending(orderByExpression);
                }
            }
            return orderedQuery;
        }

        private Expression<Func<SiteRecord, long>> GetOrderByExpression(int column)
        {
            Expression<Func<SiteRecord, long>> orderBy;

            switch (column)
            {
                case 1:
                    orderBy = e => e.hits; break;

                case 2:
                    orderBy = e => e.duplicates; break;
                
                default:
                    throw new ArgumentException();
            }
            return orderBy;
        }

        private IQueryable<SiteRecord> PagedQuery(IQueryable<SiteRecord> records, DataTableParameterModel parameters)
        {
            return records.Skip(parameters.iDisplayStart)
                          .Take(parameters.iDisplayLength);
        }

        #endregion
    }
}