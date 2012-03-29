namespace Avina.Controllers
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Avina.Extensions;
    using Avina.Models;

    public class HomeController : Controller
    {
        Repository repository = new Repository();

        public ActionResult Index()
        {
            return View("Landing");
        }

        [HttpPost]
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

        public ActionResult Index2(string q)
        {
            if (!q.IsNullEmptyOrWhitespace())
            {
                ViewBag.SearchTerms = q;
            }
            return View("Index");
        }

        public ActionResult DataTables(DataTableParameterModel model)
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

        public ActionResult Error()
        {
            return View("Error");
        }
    }
}
