namespace Avina.Controllers
{
    using System.Web.Mvc;
    using Avina.Models;
    
    public class SearchController : Controller
    {
        Repository repository = new Repository();

        public ActionResult Index(string q)
        {
            ViewBag.SearchTerm = q ?? string.Empty;
            return View(repository.Search(q));
        }
    }
}
