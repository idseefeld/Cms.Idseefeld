using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;

namespace Cms.Idseefeld.Web.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error")]
        public IActionResult Index()
        {
            if (Response.StatusCode != StatusCodes.Status200OK)
            {
                var exceptionHandlerPathFeature =
                    HttpContext.Features.Get<IExceptionHandlerPathFeature>();
                var path = exceptionHandlerPathFeature?.Path ?? string.Empty;
                if (path.StartsWith("/statuscodes"))
                {
                    //Here we render a fall back view, which should be carefully tested and have as few dependencies as possible.
                    return View();
                }

                if (Response.StatusCode == StatusCodes.Status500InternalServerError)
                {
                    return View("Error/index");
                    return Redirect("/statuscodes/500");
                }

                return Redirect("/statuscodes");
            }

            return Redirect("/");
        }
    }
}
