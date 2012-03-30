namespace Avina.Controllers.Attributes
{
    using System.Web.Mvc;

    /// <summary>
    /// This attribute allows a cross-domain request to happen by wildcarding the domain
    /// </summary>
    public class AllowCORSAttribute : ActionFilterAttribute
    {
        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            filterContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");
            filterContext.HttpContext.Response.AddHeader("Access-Control-Allow-Credentials", "false");
 	        base.OnResultExecuted(filterContext);
        }
    }
}