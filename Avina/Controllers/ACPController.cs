namespace Avina.Controllers
{
    using System.Diagnostics;
    using System.Web.Mvc;
    using System.Web.Security;
    using Avina.Extensions;
    using Avina.Models;
    using NBrowserID;

    [HandleError]
    public class ACPController : Controller
    {
        Repository repository = new Repository();

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
            Debug.WriteLine(url);
            repository.Remove(url);
            return Json(true);
        }

        [Authorize(Users = "tristan@seditious-tech.com")]
        [HttpPost]
        public ActionResult PurgeWithFilters()
        {
            repository.ApplyFiltersRetro();
            return Json(true);
        }
    }
}
