using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api;

public class LoadedCheckFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var store = context.HttpContext.RequestServices.GetService<LogStore>()!;
        
        if (!store.Loaded)
        {
            context.Result = new StatusCodeResult((int)HttpStatusCode.BadRequest);
            return;
        }
        
        base.OnActionExecuting(context);
    }
}