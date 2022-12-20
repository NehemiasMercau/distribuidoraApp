using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;



namespace DistribuidoraAPI
{
    public class SessionExpireAttribute : ActionFilterAttribute
    {
       
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = HttpContext.Current;
            // check  sessions here
            if (HttpContext.Current.Session["Dolar"] == null)
            {
                filterContext.Result = new RedirectResult("~/Account/TimeOut");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}