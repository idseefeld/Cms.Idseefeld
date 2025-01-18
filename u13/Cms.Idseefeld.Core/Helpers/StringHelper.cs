using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.PublishedModels;
using Umbraco.Extensions;
using ContentModels = Umbraco.Cms.Web.Common.PublishedModels;

namespace Cms.Idseefeld.Core.Helper
{
    public static class StringHelper
    {
        public static IHtmlEncodedString GetTeaserTextFromGrid(this IdsBlogPage blogPage, string gridAlias, IHtmlHelper Html, int maxBodyLength)
        {
            string teaserBody = blogPage.TeaserBodytext?.ToHtmlString() ?? "";

            if (string.IsNullOrEmpty(teaserBody) && !string.IsNullOrEmpty(blogPage.MetaDescription))
            {
                teaserBody = $"<p>{blogPage.MetaDescription.RemoveTitle(blogPage.GetTitleFromTeaserMetaOrModel())}</p>";
            }

            if (string.IsNullOrEmpty(teaserBody))
            {
                string gridHtml = Html.GetGridHtml(blogPage, gridAlias)
                    .ToHtmlString();
                gridHtml = gridHtml
                    .Replace("</", "~#</");
                gridHtml = gridHtml
                    .StripHtml()
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Replace("  ", "");
                gridHtml = gridHtml
                    .Replace("~# ", " ")
                    .Replace("~#", " ");
                if (!string.IsNullOrEmpty(gridHtml))
                {
                    teaserBody = $"<p>{gridHtml.RemoveTitle(blogPage.GetTitleFromTeaserMetaOrModel())}</p>";
                }

                if (teaserBody.Length > maxBodyLength)
                {
                    var lastSpacePos = teaserBody.Substring(0, maxBodyLength).LastIndexOf(" ");
                    teaserBody = $"{teaserBody[..lastSpacePos]} ...";
                }
            }

            return new HtmlEncodedString(teaserBody);
        }
        private static string RemoveTitle(this string text, string title)
        {
            var rVal = text;
            if (title.Length > 0 && rVal.InvariantStartsWith(title))
            {
                rVal = rVal.Substring(title.Length).TrimStart();
            }
            return rVal;
        }
        public static string GetTitleFromTeaserMetaOrModel(this IdsBlogPage page)
        {
            string title = !string.IsNullOrEmpty(page.TeaserTitle)
                ? page.TeaserTitle
                : !string.IsNullOrEmpty(page.MetaTitle)
                    ? page.MetaTitle
                    : page.Name ?? String.Empty;

            return title;
        }
    }
}
