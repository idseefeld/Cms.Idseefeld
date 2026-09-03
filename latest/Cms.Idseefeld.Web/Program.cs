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

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseHsts();
}

// fix security health checks with NWebsec.AspNetCore.Middleware
app.UseXfo(options => options.SameOrigin());
app.UseXContentTypeOptions();

app.UseCsp(
    options => options
    .ConnectSources(s => s
        .Self()
        .CustomSources(
            "http://localhost:55802",
            "http://localhost:63274"
            )
    )
    .DefaultSources(s => s
        .Self()
        .CustomSources(
            "cdn.jsdelivr.net",
            "static.licdn.com"
        )
    )
    .ScriptSources(s => s
        .Self()
        .UnsafeInline()
        .CustomSources(
            "cdn.jsdelivr.net",
            "cdnjs.cloudflare.com",
            "*.linkedin.com"
        )
    )
    .FormActions(s => s.Self())
    .StyleSources(s => s
        .Self()
        .UnsafeInline()
        .CustomSources(
            "cdn.jsdelivr.net",
            "static.licdn.com"
        )
    )
);

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
