namespace Avina.Models
{
    using System;
    using System.Collections.Specialized;
    using System.Text.RegularExpressions;
    using Avina.Extensions;
    using System.Web;
    using System.Net;
    using HtmlAgilityPack;
    using System.Linq;

    public class SubmissionModel
    {
        const int N_FIRSTP_LENGTH = 250 - 4;
        const int N_TITLE_LENGTH = 86 - 4;

        public SubmissionModel(string url, string requestIP)
        {
            this.Url = url;
            this.When = DateTime.UtcNow;
            this.HostIP = requestIP;
            this.GetFirstP();
        }

        public SubmissionModel(NameValueCollection requestCollection, string requestIP)
        {
            this.Referrer = requestCollection["referrer"];
            this.Url = requestCollection["url"];
            this.Title = (requestCollection["title"].Length > N_TITLE_LENGTH + 3)
                ? requestCollection["title"].Substring(0, N_TITLE_LENGTH) + "..."
                : requestCollection["title"];
            if (this.Title.IsNullEmptyOrWhitespace()) this.Title = null;
            this.HostIP = requestIP;
            this.When = DateTime.UtcNow;
            this.GetFirstP();
        }

        private void GetFirstP()
        {
            if (!this.Url.IsNullEmptyOrWhitespace())
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Url);
                    var doc = new HtmlDocument();
                    doc.Load(request.GetResponse().GetResponseStream());
                    var pElem = doc.DocumentNode.SelectSingleNode("//p");
                    var text = pElem.InnerText.Trim();
                    if (text.Length > N_FIRSTP_LENGTH + 3)
                    {
                        text = text.Substring(0, N_FIRSTP_LENGTH) + "...";
                    }
                    this.FirstP = text;
                }
                catch { }
            }
        }

        public bool IsValid
        {
            get
            {
                return !this.Url.IsNullEmptyOrWhitespace()   &&
                       !this.Title.IsNullEmptyOrWhitespace() &&
                        this.FilterValidate();
            }
        }

        public string Url { get; set; }

        public string Referrer { get; set; }

        public string Title { get; set; }

        public string HostIP { get; set; }

        public DateTime When { get; set; }

        public string FirstP { get; set; }

        /// <summary>
        /// Validates the current URL
        /// </summary>
        /// <returns>True if none of the filters were triggered, false if it matched even one</returns>
        private bool FilterValidate()
        {
            foreach (var regex in RegexFilters)
            {
                if (Regex.IsMatch(this.Url, regex))
                    return false;
            }
            return true;
        }

        public static string[] RegexFilters = new string[] {
            @"(http|https):\/\/avina",
            @"(http|https):\/\/localhost",
            @"(http|https):\/\/[w.]+google.[A-Za-z.]+\/search",
            @"(http|https):\/\/[w.]+google.[A-Za-z.]+\/webhp",
            @"(http|https):\/\/[w.]+google.[A-Za-z.]+\/imgres",
            @"(http|https):\/\/[w.]+facebook.com\/plugins\/like",
            @"(http|https):\/\/[w.]+bing.com\/search",
            @"(http|https):\/\/[A-Za-z].yahoo.com\/search",
            @"(http|https):\/\/platform.twitter",
            @"(http|https):\/\/duckduckgo.com\/?q=",
            @"(http|https):\/\/[A-Za-z0.9\-\.]+\/search",
            @"(http|https):\/\/stackoverflow.com\/questions\/tagged"
        };
    }
}