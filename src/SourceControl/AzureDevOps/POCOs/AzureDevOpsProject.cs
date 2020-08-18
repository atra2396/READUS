﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SourceControl.AzureDevOps.POCOs
{
    public class AzureDevOpsProject
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string state { get; set; }
        public int revision { get; set; }
        public string visibility { get; set; }
        public DateTime lastUpdateTime { get; set; }
    }
}
