using System;
using System.Collections.Generic;
using System.Text;

namespace SourceControl.InMemory
{
    public class MemoryMetadata
    {
        public bool HasPassword { get; set; }
        public string Password { get; set; }
        public string RootDirectory { get; set; }
    }
}
