namespace Avina.Controllers
{
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Avina.Extensions;
    using Avina.Models;

    public class SubmitController : Controller
    {
        /// <summary>
        /// When the browser extension fires, it sends a packet of JSON here to be (possibly)
        /// added to the index.
        /// </summary>
        [HttpPost]
        [AllowCrossSiteJson]
        public ActionResult Index()
        {
            Task.Factory.StartNew(() =>
            {
                var result = HttpUtility.UrlDecode(Request.InputStream.AsString());
                var submission = new SubmissionModel(HttpUtility.ParseQueryString(result), Request.UserHostAddress);
                (new Repository()).Add(submission);
            });
            
            return new HttpStatusCodeResult(200);
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

    public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Credentials", "true");
            
            base.OnActionExecuting(filterContext);
        }
    }
}
