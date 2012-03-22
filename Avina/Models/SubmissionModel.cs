using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Avina.Extensions;
namespace Avina.Models
{
    public class SubmissionModel
    {
        public SubmissionModel(string url, string requestIP)
        {
            this.Url = url;
            this.When = DateTime.UtcNow;
            this.HostIP = requestIP;
        }

        public SubmissionModel(NameValueCollection requestCollection, string requestIP)
        {
            this.Referrer = requestCollection["referrer"];
            this.Url = requestCollection["url"];
            this.Title = requestCollection["title"];
            this.HostIP = requestIP;
            this.When = DateTime.UtcNow;
        }


        public bool IsValid
        {
            get
            {
                return ! this.Url.IsNullEmptyOrWhitespace()   &&
                       ! this.Title.IsNullEmptyOrWhitespace() &&
                       !(this.Url.StartsWith("http://avina")  ||
                         this.Referrer.StartsWith("http://avina"));
            }
        }

        public string Url { get; set; }

        public string Referrer { get; set; }

        public string Title { get; set; }

        public string HostIP { get; set; }

        public DateTime When { get; set; }
    }
}