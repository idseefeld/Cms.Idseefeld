using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Dashboards;
using Umbraco.Cms.Core.DependencyInjection;

namespace Cms.Idseefeld.Core.Compositions
{
    public class RemoveDashboard : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Dashboards().Remove<ContentDashboard>();
        }
    }
}