namespace Avina.Controllers
{
    using System.Diagnostics;
    using System.Web.Http;
    using Avina.Controllers.Attributes;
    using Avina.Extensions;
    using Avina.Models;
    using Avina.Models.Requests;

    
    public class ExperimentSubmitController : ApiController
    {
        [AllowCORS]
        public POSTModel PostSubmit(POSTModel model)
        {
            ForwardIndex.Add(new SiteSubmission(model.url, model.referrer, Request.GetClientIp()));
            #if DEBUG
            Debug.WriteLine(string.Format("POST/api/submit:\t{0}\t{1}", model.url, model.referrer));
            #endif

            
            return model;
        }

        public class POSTModel
        {
            public string url { get; set; }

            public string referrer { get; set; }
        }
    }
}
