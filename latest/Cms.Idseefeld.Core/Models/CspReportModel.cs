using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Cms.Idseefeld.Core.Models
{
    public class CspReportModel
    {
        [JsonPropertyName("csp-report")]
        public CspReportDetailsModel? Report { get; set; }
    }
    public class CspReportDetailsModel
    {
        [JsonPropertyName("document-uri")]
        public string DocumentUri { get; set; } = string.Empty;

        [JsonPropertyName("referrer")]
        public string Referrer { get; set; } = string.Empty;

        [JsonPropertyName("violated-directive")]
        public string ViolatedDirective { get; set; } = string.Empty;

        [JsonPropertyName("effective-directive")]
        public string EffectiveDirective { get; set; } = string.Empty;

        [JsonPropertyName("blocked-uri")]
        public string BlockedUri { get; set; } = string.Empty;

        [JsonPropertyName("original-policy")]
        public string OriginalPolicy { get; set; } = string.Empty;

        [JsonPropertyName("disposition")]
        public string Disposition { get; set; } = string.Empty;

        [JsonPropertyName("line-number")]
        public int LineNumber { get; set; }

        [JsonPropertyName("column-number")]
        public int ColumnNumber { get; set; }

        [JsonPropertyName("source-file")]
        public string SourceFile { get; set; } = string.Empty;

        [JsonPropertyName("status-code")]
        public int StatusCode { get; set; }

        [JsonPropertyName("script-sample")]
        public string ScriptSample { get; set; } = string.Empty;
    }
}
