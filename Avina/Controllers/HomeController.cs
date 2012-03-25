namespace Avina.Controllers
{
    using System.Web.Mvc;
    using Avina.Models;
    using Avina.Extensions;

    public class HomeController : Controller
    {
        Repository repository = new Repository();

        public ActionResult Index(string q)
        {
            if (q.IsNullEmptyOrWhitespace())
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Search", new { q = q });
            }
        }

        public ActionResult DataTables(DataTableParameterModel model)
        {
            return Json(repository.DataTableQuery(model), JsonRequestBehavior.AllowGet);
        }
    }
}
