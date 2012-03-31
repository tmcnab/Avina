using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Avina.Controllers
{
    public class InfoController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Extensions()
        {
            return View();
        }
    }
}
