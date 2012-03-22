namespace Avina.Controllers
{
    using System.Linq;
    using System.Web.Mvc;
    using Avina.Models;

    public class HomeController : Controller
    {
        Repository repository = new Repository();

        public ActionResult Index()
        {
            return View(repository.GetAll().OrderByDescending(r => r.hits).ThenByDescending(r => r.duplicates));
        }

        // Not in use yet
        public ActionResult DataTables(DataTablesParameterModel model)
        {
            var dtData = from r in repository.GetAll()
                         select new { r.url, r.title, r.hits, r.duplicates };
            return Json(new {
                sEcho = model.sEcho,
                iTotalRecords = dtData.Count(),
                iTotalDisplayRecords = dtData.Count(),
                aaData = dtData.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
