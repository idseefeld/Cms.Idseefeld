//using Cms.Idseefeld.Core.Localization;
using Cms.Idseefeld.Core.Models;
using Cms.Idseefeld.Core.Services;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Globalization;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Website.Controllers;

namespace Cms.Idseefeld.Core.Controllers
{
    public class ContactFormController : SurfaceController
    {
        private readonly ILogger _logger;
        private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
        private readonly ContactMailOptions _mailOptions;
        private readonly IMailService _mailService;
        private readonly int _minEditingSconds = 9;
        public ContactFormController(ILogger<SurfaceController> logger, IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, IOptionsMonitor<GlobalSettings> globalSettings, IOptions<ContactMailOptions> mailOptions, IMailService mailService) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _globalSettings = globalSettings;
            _mailOptions = mailOptions.Value;
            _mailService = mailService;
            _logger = logger;
        }

        /// <summary>
        /// route: /umbraco/surface/{controllername}/{action}/{id}
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateUmbracoFormRouteString]
        [ValidateAntiForgeryToken]
        public IActionResult HandleSubmit(ContactFormModel formData)
        {
            // this is neccessary to prevent timer reset during invalid data round trips. See: IdsStandardPageController
            HttpContext.Session.SetString(Constances.Common.FormPosted, DateTime.Now.Ticks.ToString());

            if (formData.DataConsent && formData.DataConsentText == null)
            {
                ModelState.Remove("DataConsentText");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            _ = long.TryParse((HttpContext.Session.GetString(Constances.Common.EditStart) ?? string.Empty).ToString(), out long startTicks);
            var duration = (DateTime.Now - new DateTime(startTicks)).TotalSeconds;
            HttpContext.Session.Remove(Constances.Common.EditStart);
            HttpContext.Session.Remove(Constances.Common.FormPosted);


            var minEditing = _mailOptions.MinFormEditingInSeconds ?? _minEditingSconds;
            if (startTicks > 0 && duration > minEditing)
            {
                // only send email when post request comes 5 seconds after inital page load
                formData.HostInfo = $"Send from {Request.Host}";
                formData.EditingTime = (int)Math.Floor(duration);
                _mailService.HandleContactData(formData);
            }
            else
            {
                _logger.LogWarning("User or probably bot edited contact form in less then {seconds} seconds.", minEditing);
            }

            if (CurrentPage?.Children != null && CurrentPage.Children.Any())
            {
                return RedirectToUmbracoPage(CurrentPage.Children.First());
            }

            return RedirectToCurrentUmbracoPage();
        }


    }
}
