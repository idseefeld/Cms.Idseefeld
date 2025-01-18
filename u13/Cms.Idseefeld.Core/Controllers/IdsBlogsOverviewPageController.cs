using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace Cms.Idseefeld.Core.Controllers
{
    public class IdsBlogsOverviewPageController : RenderController
    {
        private readonly ILogger _logger;
        public IdsBlogsOverviewPageController(ILogger<RenderController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor) : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _logger = logger;
        }

        public IActionResult IdsBlogsOverviewPage(ContentModel model)
        {

            return View(model);
        }
    }
}
