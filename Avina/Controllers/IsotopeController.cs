namespace Avina.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using Avina.Models;
    using Avina.Extensions;

    public class IsotopeController : Controller
    {
        Repository repository = new Repository();

        public ActionResult Index()
        {
            var items = repository.GetAll().OrderByDescending(p => p.submitted)
                                           .ThenByDescending(p => p.duplicates)
                                           .ThenByDescending(p => p.hits);
            return View(items);
        }
    }
}
