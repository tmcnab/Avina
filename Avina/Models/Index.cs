namespace Avina.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Avina.Extensions;
    using Avina.Models.Search;

    /// <summary>
    /// Main interface (once Repository.cs is defactored) to the search functionality of Avina
    /// </summary>
    public class Index
    {

        public IEnumerable<SiteRecord> Search(string queryString)
        {
            if (queryString.IsNullEmptyOrWhitespace())
                return new List<SiteRecord>();

            #if DEBUG
            Debug.WriteLine(string.Format("Index::Search({0})", queryString));
            #endif

            return InvertedIndex.ApplyTerms(queryString.ToLowerInvariant()
                                                       .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                       .ToList());
        }
    }
}