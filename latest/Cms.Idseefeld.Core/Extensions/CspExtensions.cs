using Cms.Idseefeld.Core.Models;
using Examine;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using NWebsec.Core.Common.Middleware.Options;
using Polly;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace Cms.Idseefeld.Core.Extensions
{
    public static class CspExtensions
    {
        private static IFluentCspOptions GetCspOptions(this IFluentCspOptions options, string? reportUri)
        {
            options
                .DefaultSources(s => s
                    .Self()
                    .CustomSources(
                        "*.umbraco.com"
                    )
                )
                .ImageSources(s => s
                    .Self()
                    .CustomSources(
                        "cdn.jsdelivr.net",
                        "*.licdn.com",
                        "data:",
                        "*.umbraco.com"
                    )
                )
                .ScriptSources(s => s
                    .Self()
                    .UnsafeInline()
                    .CustomSources(
                        "cdn.jsdelivr.net",
                        "cdnjs.cloudflare.com",
                        "*.linkedin.com",
                        "*.licdn.com"
                    )
                )
                .StyleSources(s => s
                    .Self()
                    .UnsafeInline()
                    .CustomSources(
                        "cdn.jsdelivr.net",
                        "cdnjs.cloudflare.com",
                        "*.licdn.com"
                    )
                );

            if (!string.IsNullOrEmpty(reportUri))
            {
                options.ReportUris(r => r.Uris(reportUri));
            }

            return options;
        }
        public static void AddCsp(this IApplicationBuilder app, bool cspReportOnly, string? reportUri = null)
        {
            // CSP (Content Security Policy) configuration
            if (cspReportOnly)
            {
                app.UseCspReportOnly(options => options.GetCspOptions(reportUri));
            }
            else
            {
                app.UseCsp(options => options.GetCspOptions(reportUri));
            }
        }

        public static async Task<IResult> MapCspReport(
            this HttpContext context,
            ILogger logger,
            string contentRoot,
            string cspReportDirectory
            )
        {
            // CSP violation report endpoint
            using var reader = new StreamReader(context.Request.Body);
            var report = await reader.ReadToEndAsync();
            if (string.IsNullOrEmpty(report))
            {
                return Results.BadRequest("CSP report is empty");
            }

            try
            {
                var reportDetails = JsonSerializer.Deserialize<CspReportModel>(report)?.Report
                       ?? new CspReportDetailsModel();
                if (reportDetails.ViolatedDirective == "connect-src")
                {
                    if (reportDetails.SourceFile.EndsWith("_vs/browserLink")
                        || reportDetails.SourceFile.EndsWith("_framework/aspnetcore-browser-refresh.js"))
                    {
                        // ignore connect-src violations for the source file
                        return Results.NoContent(); // 204 – browsers expect no body
                    }
                }
            }
            catch (Exception)
            {
                logger.LogWarning("Failed to deserialize CSP report: {Report}", report);
                return Results.BadRequest("Failed to deserialize CSP report");
            }

            try
            {
                // write the report to a file in the TEMP folder
                var dirPath = $"{contentRoot}{cspReportDirectory}";
                Directory.CreateDirectory(dirPath);
                var hash = report.GenerateMd5();
                var filePath = $"{dirPath}\\csp-report-{hash}.json";
                if (!File.Exists(filePath))
                {
                    var fso = File.Open(filePath, FileMode.CreateNew, FileAccess.Write);
                    using (var writer = new StreamWriter(fso, Encoding.UTF8))
                    {
                        await writer.WriteLineAsync(report);
                    }
                    fso.Close();
                }
            }
            catch (Exception)
            {

                throw;
            }

            return Results.Content(report, "application/json", Encoding.UTF8);
        }
    }
}
