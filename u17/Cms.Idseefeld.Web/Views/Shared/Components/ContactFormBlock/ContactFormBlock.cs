using Cms.Idseefeld.Core.Models;
using Cms.Idseefeld.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;

namespace Cms.Idseefeld.Web.Views.Shared.Components.ContactFormBlock
{
    public class ContactFormBlock : ViewComponent
    {
        public async Task<IViewComponentResult>? InvokeAsync(UmbracoHelper umbracoHelper, PublishedContentModel page)
        {
            var contactFormModel = new ContactFormModel(); 

            return View(contactFormModel);
        }
    }
}
