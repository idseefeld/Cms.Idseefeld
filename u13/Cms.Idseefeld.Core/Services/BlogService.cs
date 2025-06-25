using Cms.Idseefeld.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.PublishedModels;

namespace Cms.Idseefeld.Core.Services
{
    public class BlogService
    {
        public BlogService() { }

        public static IEnumerable<IdsBlogPage> GetBlogs(UmbracoHelper umbraco, IdsBlogsOverviewPage? page = null)
        {
            var blogsOverviewPage = page ?? umbraco.ContentAtRoot()
                .FirstOrDefault()?
                .Children<IdsBlogsOverviewPage>()?
                .FirstOrDefault(c => c.IsVisible());

            var blogs = (blogsOverviewPage?.Children<IdsBlogPage>()?
                .Where(c => c.IsVisible()) ?? Array.Empty<IdsBlogPage>())
                .OrderByDescending(b => b.DisplayDate)
                .ThenByDescending(b => b.UpdateDate);

            return blogs;
        }

        public static IEnumerable<BlogListBlogsItem> GetBlogListBlogsItems(UmbracoHelper umbraco, PublishedContentModel page, bool excluedCurrent = false)
        {
            IdsBlogsOverviewPage? overviewPage = (page is IdsBlogsOverviewPage)
                ? page as IdsBlogsOverviewPage
                : page.Parent as IdsBlogsOverviewPage;

            var allItems = GetBlogs(umbraco, overviewPage);
            var pageUrl = page?.Url() ?? string.Empty;
            if (page != null && excluedCurrent)
            {
                allItems = allItems
                    .Where(b => !b.Url().InvariantEquals(page.Url()));
            }
            return allItems
                .Select(s =>
                {
                    var url = s.Url() ?? string.Empty;
                    var isCurrent = url.InvariantEquals(pageUrl);
                    return new BlogListBlogsItem()
                    {
                        Label = s.TeaserTitle ?? s.Name ?? string.Empty,
                        Url = url,
                        IsCurrent = isCurrent
                    };
                });
        }
    }
}
