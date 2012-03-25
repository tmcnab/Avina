using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.Mvc;
using Avina.Extensions;
using NBrowserID;

namespace Avina.Controllers
{
    public class ACPController : Controller
    {
        public ActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SignIn(string assertion)
        {
            var authentication = new BrowserIDAuthentication();
            var verificationResult = authentication.Verify(assertion);
            if (verificationResult.IsVerified)
            {
                string email = verificationResult.Email;
                FormsAuthentication.SetAuthCookie(email, false);
                return Json(new { email });
            }

            return Json(null);
        }

        [Authorize(Users="tristan@seditious-tech.com")]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Users = "tristan@seditious-tech.com")]
        [HttpPost]
        public ActionResult DeleteUrl()
        {
            var url = Request.InputStream.AsString();
            return Json(true);
        }
    }
}
