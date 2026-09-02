using Cms.Idseefeld.Core.Models;
using Cms.Idseefeld.Core.Services;
using Microsoft.AspNetCore.DataProtection;
using Umbraco.Cms.Core.Services;

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

// fix security health checks with NWebsec.AspNetCore.Middleware
app.UseXfo(options => options.SameOrigin());
app.UseXContentTypeOptions();

await app.BootUmbracoAsync();

app.UseHttpsRedirection();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        //u.UseInstallerEndpoints();
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

await app.RunAsync();
