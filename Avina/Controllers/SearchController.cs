namespace Avina.Controllers
{
    using System.Web.Mvc;
    using Avina.Extensions;
    using Avina.Models;

    public class SearchController : Controller
    {
        Repository repository = new Repository();
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
