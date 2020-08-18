using System;
using System.Collections.Generic;
using System.Text;

namespace SourceControl.AzureDevOps.POCOs
{
    public class AzureDevOpsRepository
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public AzureDevOpsProject project { get; set; }
        public string remoteUrl { get; set; }
        public string sshUrl { get; set; }
        public string webUrl { get; set; }
    }
}
