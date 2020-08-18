using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourceControl.AzureDevOps.POCOs
{
    public class AzureDevOpsProjectsResponse
    {
        public int count { get; set; }
        [JsonProperty("value")]
        public List<AzureDevOpsProject> Projects { get; set; }
    }
}
