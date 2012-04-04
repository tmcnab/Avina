namespace Avina.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using Avina.Controllers;

    public static class ExceptionHelpers
    {
        public static Exception SendToACP(this Exception ex)
        {
            ACPController.ExceptionList.Add(ex);
            return ex;
        }
    }
}