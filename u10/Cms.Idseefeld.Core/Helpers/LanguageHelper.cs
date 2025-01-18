using Cms.Idseefeld.Core.Models;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;


namespace Cms.Idseefeld.Core.Helpers
{
    public static class LanguageHelper
    {
        /// <summary>
        /// Generates the list of languages for the page dropdown. If there is a parallel page using same culture then this is used, otherwise it falls back to the root URL.
        /// </summary>
        /// <param name="currentPage">The currently viewed page</param>
        /// <param name="currentCulture">The current culture</param>
        /// <returns>A list of languages and cultures</returns>
        public static IEnumerable<CultureUrl>? GetCurrentLanguageList(IPublishedContent? currentPage, string currentCulture, IAppPolicyCache cachePolicy)
        {
            if (currentPage == null) return null;

            var rootPage = currentPage.AncestorOrSelf<IdsRootPage>();
            if (rootPage == null) return null;

            var rootCultures = GetRootCultures(rootPage, cachePolicy);
            if (rootCultures == null) return null;

            var pageCultures = currentPage.Cultures.Select(x => x.Value);

            var mapped = new List<CultureUrl>(5);

            foreach (var root in rootCultures)
            {
                var pageMatch = pageCultures.FirstOrDefault(c => c.Culture == root.Culture);

                CultureUrl? mappedPage = null;

                if (pageMatch != null)
                {
                    string url = currentPage.Url(pageMatch.Culture);

                    if (url != "#")
                    {
                        mappedPage = new CultureUrl()
                        {
                            Culture = pageMatch.Culture,
                            Url = url
                        };
                    }
                }

                mappedPage ??= new CultureUrl()
                {
                    Culture = root.Culture,
                    Url = rootPage.Url(root.Culture)
                };

                mappedPage.Current = mappedPage.Culture.InvariantEquals(currentCulture);

                mapped.Add(mappedPage);
            }

            return mapped;
        }

        private static IEnumerable<PublishedCultureInfo>? GetRootCultures(IPublishedContent currentPage, IAppPolicyCache cachePolicy)
        {
            var rootCultures = AppCacheExtensions.GetCacheItem<IEnumerable<PublishedCultureInfo>>(cachePolicy, "RootCultures");
            if (rootCultures != null) return rootCultures;

            rootCultures = currentPage.Cultures.Select(x => x.Value)?.ToArray();
            return AppCacheExtensions.GetCacheItem(cachePolicy, "RootCultures", () => rootCultures);
        }
    }
}
