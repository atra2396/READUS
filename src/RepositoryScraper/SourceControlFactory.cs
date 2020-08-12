using DomainObjects;
using SourceControl;
using SourceControl.AzureDevOps;
using SourceControl.GitHub;
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
                case SupportedSystems.AzureDevOps:
                    return new AzureDevOpsSourceControlAccessor();
                case SupportedSystems.GitHub:
                    return new GitHubSourceControlAccessor();
                default:
                    throw new NotSupportedException($"SCM {system.ToString()} is not yet supported");
            }
        }
    }
}
