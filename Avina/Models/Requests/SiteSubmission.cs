namespace Avina.Models.Requests
{
    using System;
    using System.Text.RegularExpressions;
    using Avina.Extensions;
    using Avina.Models;

    public class SiteSubmission
    {
        public SiteSubmission() { }

        public SiteSubmission(string url, string referrerUrl, string ipAddress)
        {
            this.Submitted = DateTime.UtcNow;
            this.SubmitterIP = ipAddress;
            this.Url = url;
            this.Referrer = referrerUrl;
        }

        public string Url { get; set; }

        public string Referrer { get; set; }

        public DateTime Submitted { get; set; }

        public string SubmitterIP { get; set; }

        public bool IsValid
        {
            get
            {
                return UrlFilterValidate();
            }
        }

        private bool UrlFilterValidate()
        {
            if (!this.Url.IsNullEmptyOrWhitespace())
            {
                foreach (var regex in ForwardIndex.UrlFilters)
                {
                    if (Regex.IsMatch(this.Url, regex))
                        return false;
                }

                return ReferrerValidate();
            }
            return false;
        }

        private bool ReferrerValidate()
        {
            if (!this.Referrer.IsNullEmptyOrWhitespace())
            {
                return !(Regex.IsMatch(this.Referrer, @"(http|https):\/\/avina") ||
                         Regex.IsMatch(this.Referrer, @"(http|https):\/\/localhost"));
            }
            return true;
        }

        public override string ToString()
        {
            return string.Format("{0}(URL):{1}(REF):{2}(IP)", this.Url, this.Referrer, this.SubmitterIP);
        }
    }
}