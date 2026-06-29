using Cms.Idseefeld.Core.Models;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common;
using Umbraco.Cms.Web.Common.ActionsResults;
using Umbraco.Cms.Web.Common.Filters;
using Umbraco.Cms.Web.Common.Security;
using Umbraco.Cms.Web.Website.Controllers;
using Umbraco.Extensions;

namespace Cms.Idseefeld.Core.Controllers;

public class LoginController : SurfaceController
{
    private readonly IMemberManager _memberManager;

    private readonly IMemberSignInManager _signInManager;

    private readonly ITwoFactorLoginService _twoFactorLoginService;

    private readonly IDictionaryItemService? _dictionaryItemService;

    [ActivatorUtilitiesConstructor]
    public LoginController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger, IPublishedUrlProvider publishedUrlProvider, IMemberSignInManager signInManager, IMemberManager memberManager, ITwoFactorLoginService twoFactorLoginService, IDictionaryItemService dictionaryItemService)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _signInManager = signInManager;
        _memberManager = memberManager;
        _twoFactorLoginService = twoFactorLoginService;
        _dictionaryItemService = dictionaryItemService;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ValidateUmbracoFormRouteString]
    public async Task<IActionResult> HandleLogin([Bind(new string[] { }, Prefix = "loginModel")] LoginModel model)
    {
        if (!ModelState.IsValid)
        {
            return CurrentUmbracoPage();
        }

        MergeRouteValuesToModel(model);
        Microsoft.AspNetCore.Identity.SignInResult signInResult = await _signInManager.PasswordSignInAsync(model.Username ?? string.Empty, model.Password ?? string.Empty, model.RememberMe, lockoutOnFailure: true);
        if (signInResult.Succeeded)
        {
            TempData["LoginSuccess"] = true;
            if (!model.RedirectUrl.IsNullOrWhiteSpace())
            {
                return Redirect(Url.IsLocalUrl(model.RedirectUrl) 
                    ? model.RedirectUrl 
                    : CurrentPage!.AncestorOrSelf(1).Url(PublishedUrlProvider));
            }

            return RedirectToCurrentUmbracoUrl();
        }

        string errorMessage = string.Empty;
        string isoCode = Thread.CurrentThread.CurrentCulture.Name;
        if (signInResult.RequiresTwoFactor)
        {
            MemberIdentityUser? memberIdentityUser = await _memberManager.FindByNameAsync(model.Username ?? string.Empty);
            if (memberIdentityUser == null)
            {
                return NotFound($"No local member found for username {model.Username}");
            }

            IEnumerable<string> providerNames = await _twoFactorLoginService.GetEnabledTwoFactorProviderNamesAsync(memberIdentityUser.Key);
            base.ViewData.SetTwoFactorProviderNames(providerNames);
        }
        else if (signInResult.IsLockedOut)
        {

            var dictValue = _dictionaryItemService?.GetAsync("MemberLockedOut").Result;
            errorMessage = dictValue?.Translations
                .FirstOrDefault(t => t.LanguageIsoCode == isoCode)?.Value ?? "MemberLockedOut";
        }
        else if (signInResult.IsNotAllowed)
        {
            var dictValue = _dictionaryItemService?.GetAsync("MemberNotAllowed").Result;
            errorMessage = dictValue?.Translations
                .FirstOrDefault(t => t.LanguageIsoCode == isoCode)?.Value ?? "MemberNotAllowed";
        }
        else
        {
            var dictValue = _dictionaryItemService?.GetAsync("InvalidLogin").Result;
            errorMessage = dictValue?.Translations
                .FirstOrDefault(t => t.LanguageIsoCode == isoCode)?.Value ?? "InvalidLogin";
        }
        if (!string.IsNullOrEmpty(errorMessage))
        {
            ModelState.AddModelError("loginModel", errorMessage);
            ViewData.Add("LoginFailed", new HtmlString(errorMessage));
        }
        return CurrentUmbracoPage();
    }

    //
    // Summary:
    //     We pass in values via encrypted route values so they cannot be tampered with
    //     and merge them into the model for use
    //
    // Parameters:
    //   model:
    private void MergeRouteValuesToModel(LoginModel model)
    {
        if (RouteData.Values.TryGetValue("RedirectUrl", out object? value))
        {
            model.RedirectUrl = value?.ToString();
        }
    }
}