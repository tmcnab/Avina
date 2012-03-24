namespace Avina.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Avina.Models;

    public class HomeController : Controller
    {
        Repository repository = new Repository();

        public ActionResult Index(string q = "")
        {
            IEnumerable<SiteRecord> model;
            //if (q.IsNullEmptyOrWhitespace())
            //{
                model = repository.GetAll()
                            .OrderByDescending(r => r.hits)
                            .ThenByDescending(r => r.duplicates)
                            .Take(50);
            //}
            //else
            //{
            //    model = repository.Search(q);
            //}
            return View(model);
        }

        public ActionResult DataTables(DataTableParameterModel model)
        {
            return Json(repository.DataTableQuery(model), JsonRequestBehavior.AllowGet);
        }
    }
}
