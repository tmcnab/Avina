using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Avina.Models;

namespace Avina.Controllers
{
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
