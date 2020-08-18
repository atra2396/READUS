using System;
using System.Collections.Generic;
using System.Text;

namespace SourceControl.AzureDevOps.POCOs
{
    public class Item
    {
        public string objectId { get; set; }
        public string gitObjectType { get; set; }
        public string commitId { get; set; }
        public string path { get; set; }
        public bool isFolder { get; set; }
        public ContentMetadata contentMetadata { get; set; }
        public string url { get; set; }
    }
}
