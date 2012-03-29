namespace Avina.Controllers
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Avina.Models;
    using System.Linq;
    
    public class SearchController : Controller
    {
        Repository repository = new Repository();

        public ActionResult Index(string q)
        {
            ViewBag.SearchTerm = q ?? string.Empty;
            return View(repository.Search(q));
        }

        // This whole thing coul be refactored
        public ActionResult Ajax(DataTableParameterModel model)
        {
            long nRecords = 0;
            var results = repository.DataTableQuery(model, out nRecords);
            var aaData = new List<object>();
            foreach (var item in results)
            {
                aaData.Add(new object[] {
                    new[] { item.url, item.title, item.textPreview ?? string.Empty },
                    item.hits,
                    item.duplicates
                });
            }

            return Json(new
            {
                sEcho = model.sEcho,
                iTotalRecords = nRecords,
                iTotalDisplayRecords = nRecords,
                aaData = aaData.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }


    }
}
