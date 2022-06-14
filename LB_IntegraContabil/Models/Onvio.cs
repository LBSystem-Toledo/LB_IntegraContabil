using System;
using System.Collections.Generic;

namespace LB_IntegraContabil.Models
{
    public class TokenONVIO
    {
        public string access_token { get; set; } = string.Empty;
        public string escopo { get; set; } = string.Empty;
        public string token_type { get; set; } = string.Empty;
        public string expires_in { get; set; } = string.Empty;
        public DateTime Dt_token { get; set; } = DateTime.Now;
    }
    public class IntegrationKey
    {
        public string integrationKey { get; set; } = string.Empty;
    }
    public class StatusV1Dto
    {
        public string code { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
    }
    public class RetornoONVIO
    {
        public DateTime lastStatusOn { get; set; }
        public string apiVersion { get; set; }
        public bool boxeFile { get; set; }
        public string id { get; set; }
        public StatusV1Dto status { get; set; }
    }
    public class FileV1Dto
    {
        public StatusV1Dto apiStatus { get; set; }
        public StatusV1Dto boxeStatus { get; set; }
        public string lastApiStatusOn { get; set; } = string.Empty;
        public string lastBoxeStatusOn { get; set; } = string.Empty;
    }
    public class BatchV1Dto
    {
        public string id { get; set; } = string.Empty;
        public string apiVersion { get; set; } = string.Empty;
        public bool boxeFile { get; set; } = false;
        public List<FileV1Dto> filesExpanded { get; set; }
        public StatusV1Dto status { get; set; }
        public string lastStatusOn { get; set; } = string.Empty;
    }
}
