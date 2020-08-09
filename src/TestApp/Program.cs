using DomainObjects;
using Newtonsoft.Json;
using Queueing;
using Queueing.Messages;
using RepositoryScraper;
using SourceControl.AzureDevOps;
using SourceControl.InMemory;
using Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var docs = new MemoryRepository<Document>();
            var repos = new MemoryRepository<Repository>();
            var orgs = new MemoryRepository<Organization>();

            var newOrg = new Organization()
            {
                Created = DateTime.UtcNow,
                Updated = DateTime.UtcNow,
                Name = "zhekau",
            };
            orgs.Add(newOrg);

            var repoData = new MemoryMetadata()
            {
                HasPassword = true,
                Password = "Pa$$w0rd",
                RootDirectory = @"C:\Users\alija\Documents\Programming\Test%20Project",
            };

            //In AzureDevops, the hierarchy is as follows:
            //Organization -> Project -> Repo -> Items
            var metadata = new AzureDevOpsMetadata
            {
                PersonalAccessToken = "{YOUR PAT}",
            };

            var newRepo = new Repository()
            {
                Name = "Test Repo",
                OrganizationId = newOrg.Id,
                OrganizationName = newOrg.Name,
                ProjectName = "{YOUR PROJECT NAME}",
                SCM = SupportedSystems.AzureDevOps,
                CustomRepositoryInformation = JsonConvert.SerializeObject(metadata)
            };

            repos.Add(newRepo);

            var db = new MemoryDataContext(docs, orgs, repos);

            var queue = new MemoryQueue<RepositoryUpdatedMessage>();

            while (true)
            {
                queue.PushMessage(new RepositoryUpdatedMessage(newRepo.Id));

                var scraper = new Scraper(db);
                scraper.ScrapeRepository(queue.PopMessage()).Wait();

                Display(db);
                Thread.Sleep(30000);
            }


        }

        private static void Display(IDataContext ctx)
        {
            Console.WriteLine($"Documents at {DateTime.Now}:");
            foreach (var doc in ctx.Documents.GetWhere(x => true))
            {
                Console.WriteLine(JsonConvert.SerializeObject(doc));
            }
        }
    }
}
