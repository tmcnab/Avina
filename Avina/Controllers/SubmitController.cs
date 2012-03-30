namespace Avina.Controllers
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Avina.Controllers.Attributes;
    using Avina.Extensions;
    using Avina.Models;
    using Avina.Models.Requests;

    public class SubmitController : Controller
    {
        /// <summary>
        /// When the browser extension fires, it sends a packet of JSONP here to be (possibly)
        /// added to the index.
        /// </summary>
        [HttpPost]
        [AllowCORS]
        public ActionResult Index(POSTModel model)
        {
            ForwardIndex.Add(new SiteSubmission(model.url, model.referrer, Request.UserHostAddress));
            
            #if DEBUG
            Debug.WriteLine(string.Format("POST/api/submit:\t{0}\t{1}", model.url, model.referrer));
            #endif

            return new HttpStatusCodeResult(200);
        }

        public class POSTModel
        {
            public string url { get; set; }

            public string referrer { get; set; }
        }

        /// <summary>
        /// When a user clicks on an index link, this action adds a +1 to the number of times
        /// the link has been click by an Avina user.
        /// </summary>
        [HttpPost]
        public ActionResult Click()
        {
            Task.Factory.StartNew(() =>
            {
                (new Repository()).IncrementClick(Request.InputStream.AsString());
            });

            return new HttpStatusCodeResult(200);
        }
    }
}
