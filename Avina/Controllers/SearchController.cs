namespace Avina.Controllers
{
    using System.Web.Mvc;
    using Avina.Extensions;
    using Avina.Models;
    using System.Diagnostics;

    /// <summary>
    /// The landing page / results page controller
    /// </summary>
    public class SearchController : Controller
    {
        Index searchIndex = new Index();

        public ActionResult Index(string q)
        {
            if (q.IsNullEmptyOrWhitespace())
            {
                return View("Landing");
            }

            ViewBag.SearchTerm = q;
            return View(searchIndex.Search(q));
        }
    }
}
