//using Cms.Idseefeld.Core.Localization;
using Cms.Idseefeld.Core.Models;
using Cms.Idseefeld.Core.Services;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Website.Controllers;

namespace Cms.Idseefeld.Core.Controllers
{
    public class ContactFormController : SurfaceController
    {
        private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
        private readonly ContactMailOptions _mailOptions;
        private readonly IMailService _mailService;
        public ContactFormController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, IOptionsMonitor<GlobalSettings> globalSettings, IOptions<ContactMailOptions> mailOptions, IMailService mailService) : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
        {
            _globalSettings= globalSettings;
            _mailOptions = mailOptions.Value;
            _mailService = mailService;
        }
        /// <summary>
        /// route: /umbraco/surface/{controllername}/{action}/{id}
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateUmbracoFormRouteString]
        public IActionResult HandleSubmit(ContactFormModel formData)
        {
            if (formData.DataConsent && formData.DataConsentText == null)
            {
                ModelState.Remove("DataConsentText");
            }
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            formData.HostInfo = $"Send from {Request.Host}";
            _mailService.HandleContactData(formData);

            if (CurrentPage?.Children != null && CurrentPage.Children.Any())
            {
                return RedirectToUmbracoPage(CurrentPage.Children.First());
            }

            return RedirectToCurrentUmbracoPage();
        }

        
    }
}
