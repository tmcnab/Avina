namespace Avina.Controllers
{
    using System.Web.Mvc;
    using Avina.Models;

    public class HomeController : Controller
    {
        Repository repository = new Repository();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DataTables(DataTableParameterModel model)
        {
            return Json(repository.DataTableQuery(model), JsonRequestBehavior.AllowGet);
        }
    }
}
