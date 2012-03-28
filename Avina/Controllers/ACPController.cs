namespace Avina.Controllers
{
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Avina.Extensions;
    using Avina.Models;

    [Authorize(Users = "tristan@seditious-tech.com")]
    public class ACPController : Controller
    {
        Repository repository = new Repository();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ViewIndex()
        {
            return View(InvertedIndex.GetIndex());
        }

        #region Ajax Handlers

        [HttpPost]
        public ActionResult DeleteUrl()
        {
            var url = Request.InputStream.AsString();
            Debug.WriteLine(url);
            repository.Remove(url);
            return Json(true);
        }

        [HttpPost]
        public ActionResult PurgeWithFilters()
        {
            repository.ApplyFiltersRetro();
            return Json(true);
        }

        [HttpGet]
        public ActionResult FiltersGet()
        {
            return Json(SubmissionModel.RegexFilters, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RebuildIndex()
        {
            if (!InvertedIndex.Rebuilding)
            {
                Task.Factory.StartNew(() => InvertedIndex.Rebuild(true));
                return Json(true);
            }
            else
            {
                return Json(false);
            }
        }

        [HttpGet]
        public ActionResult IndexStatus()
        {
            return Json(new {
                totalEntries = InvertedIndex.TotalEntries,
                totalItems = InvertedIndex.ProcessingTotal,
                currentItems = InvertedIndex.ProcessingCurrent,
                isRebuilding = InvertedIndex.Rebuilding,
                avgQueryTime = InvertedIndex.TAverageQueryTime
            } , JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}
