namespace Avina.Extensions
{
    using System.Net.Http;
    using System.Web;

    public static class HttpRequestMessageHelpers
    {
        public static string GetClientIp(this HttpRequestMessage request)
        {   
            if (request.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)request.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            else
            {
                return null;
            }
        }
    }
}