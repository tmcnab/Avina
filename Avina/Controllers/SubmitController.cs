using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Diagnostics;
using Avina.Models;

namespace Avina.Controllers
{
    public class SubmitController : Controller
    {
        Repository repository = new Repository();

        [HttpPost]
        [AllowCrossSiteJson]
        public ActionResult Index()
        {
            var result = HttpUtility.UrlDecode(Request.InputStream.AsString());
            var submission = new SubmissionModel(HttpUtility.ParseQueryString(result), Request.UserHostAddress);
            repository.Add(submission);
            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        public ActionResult Click()
        {
            repository.IncrementClick(Request.InputStream.AsString());
            return new HttpStatusCodeResult(200);
        }
    }

    public static class Helpers
    {
        public static string AsString(this Stream s)
        {
            using (StreamReader reader = new StreamReader(s))
            {
                return reader.ReadToEnd();
            }
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
