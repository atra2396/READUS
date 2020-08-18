using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourceControl.AzureDevOps.POCOs
{
    public class AzureDevOpsRepositoryResponse
    {
        [JsonProperty("value")]
        public List<AzureDevOpsRepository> repositories { get; set; }
        public int count { get; set; }
    }
}
