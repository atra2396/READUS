using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SourceControl.AzureDevOps.POCOs
{
    public class ItemsResponse
    {
        public int count { get; set; }
        [JsonProperty("value")]
        public List<Item> Items { get; set; }
    }
}
