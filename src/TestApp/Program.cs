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

            var metadata = new AzureDevOpsMetadata
            {
                PersonalAccessToken = "dqqom4g5tcepdzkqs3xlzaoach3l7iljvwd2cdvftcieydlullyq",
            };

            var newRepo = new Repository()
            {
                Name = newOrg.Name,
                OrganizationId = newOrg.Id,
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
