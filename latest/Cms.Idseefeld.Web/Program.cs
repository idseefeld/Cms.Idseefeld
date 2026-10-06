using Cms.Idseefeld.Core.Extensions;
using Cms.Idseefeld.Core.Models;
using Cms.Idseefeld.Core.Services;
using Microsoft.AspNetCore.DataProtection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

var contentRoot = builder.Environment.ContentRootPath;
var config = builder.Configuration;

builder
    .Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo($"{contentRoot}\\umbraco\\data\\TEMP"));

builder.Services.Configure<ContactMailOptions>(config.GetSection(ContactMailOptions.ContactMail));

var contactMailConfig = config.GetSection(ContactMailOptions.ContactMail).Get<ContactMailOptions>();
builder.Services.AddTransient<IMailService, MailKitService>();

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

//if (builder.Environment.IsDevelopment())
//{
//    app.UseDeveloperExceptionPage();
//}
//else
//{
//    app.UseHsts();
//}

//// fix security health checks with NWebsec.AspNetCore.Middleware
//app.UseXfo(options => options.SameOrigin());
//app.UseXContentTypeOptions();

await app.BootUmbracoAsync();

//app.UseHttpsRedirection();

//string reportUri = config.GetValue<string?>("CspProtection:ReportUri") ?? "/csp-report";
//string? cspReportDirectory = config.GetValue<string?>("CspProtection:ReportDirectory");
//if(string.IsNullOrEmpty(cspReportDirectory))
//{
//    cspReportDirectory = "\\umbraco\\data\\TEMP\\csp-reports";
//}
//bool enableCspProtection = config.GetValue<bool?>("CspProtection:EnableCspProtection") ?? false;
//bool cspReportOnly = config.GetValue<bool?>("CspProtection:CspReportOnly") ?? false;
//bool enableReport= config.GetValue<bool?>("CspProtection:EnableReport") ?? false;
//string reservedPaths = config.GetValue<string?>("Umbraco:CMS:Global:ReservedPaths") ?? string.Empty;
//if (enableCspProtection)
//{
//    app.AddCsp(cspReportOnly, reportUri);

//    if (enableReport && !string.IsNullOrEmpty(reportUri))
//    {
//        if(!reservedPaths.Contains(reportUri))
//        {
//            reservedPaths += $",{reportUri}";
//            config["Umbraco:CMS:Global:ReservedPaths"] = reservedPaths;
//        }
//        app.MapPost(reportUri, async (HttpContext context, ILogger<Program> logger) => await context.MapCspReport(logger, contentRoot, cspReportDirectory));
//    }
//}

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync(); 