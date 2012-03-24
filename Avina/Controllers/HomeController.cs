namespace Avina.Controllers
{
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using Avina.Models;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public class HomeController : Controller
    {
        Repository repository = new Repository();

        public ActionResult Index()
        {
            var model = repository.GetAll()
                                  .OrderByDescending(r => r.hits)
                                  .ThenByDescending(r => r.duplicates)
                                  .Take(50);
            return View(model);
        }

        public ActionResult DataTables(DataTableParameterModel model)
        {
            System.Diagnostics.Debug.WriteLine(JsonConvert.SerializeObject(model));
            return Json(repository.DataTableQuery(model), JsonRequestBehavior.AllowGet);
        }
    }
}
