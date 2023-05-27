using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Web.Common.Controllers;

namespace Cms.Idseefeld.Core.Controllers
{
    public  class IdsStandardPageController : RenderController
    {
        private readonly ILogger _logger;

        public IdsStandardPageController(ILogger<RenderController> logger, ICompositeViewEngine compositeViewEngine, IUmbracoContextAccessor umbracoContextAccessor) : base(logger, compositeViewEngine, umbracoContextAccessor)
        {
            _logger = logger;
        }
        /// <summary>
        /// What is Umbraco Route Hijacking?
        /// see: https://docs.umbraco.com/umbraco-cms/reference/routing/custom-controllers
        /// </summary>
        /// <returns></returns>
        public override IActionResult Index()
        {
            HttpContext.Session.SetString(Constances.Common.EditStart, DateTime.Now.Ticks.ToString());

            return CurrentTemplate(CurrentPage);
        }
    }
}
