using Cms.Idseefeld.Core.Models;
using Cms.Idseefeld.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Cms.Idseefeld.Web.Views.Shared.Components.BlogListBlogsSidebar
{
    public class BlogListBlogsSidebar : ViewComponent
    {
        public async Task<IViewComponentResult>? InvokeAsync(UmbracoHelper umbracoHelper, IdsBlogPage page)
        {
            var items = await GetItems(umbracoHelper, page);

            return View(items);
        }

        private async Task<IEnumerable<BlogListBlogsItem>> GetItems(UmbracoHelper umbracoHelper, IdsBlogPage page)
        {
            var items = BlogService.GetBlogListBlogsItems(umbracoHelper, page);
            return await Task.FromResult(items);
        }
    }
}
