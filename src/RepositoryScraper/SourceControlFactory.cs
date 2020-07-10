using DomainObjects;
using SourceControl;
using SourceControl.InMemory;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryScraper
{
    public static class SourceControlFactory
    {
        public static ISourceControlAccessor GetSourceControlAccessor(SupportedSystems system)
        {
            switch (system)
            {
                case SupportedSystems.Memory:
                    return new MemorySourceControlAccessor();
                default:
                    throw new NotSupportedException($"SCM {system.ToString()} is not yet supported");
            }
        }
    }
}
