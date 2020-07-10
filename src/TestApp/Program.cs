using DomainObjects;
using Newtonsoft.Json;
using Queueing;
using Queueing.Messages;
using RepositoryScraper;
using SourceControl.InMemory;
using Storage;
using System;
using System.Threading;

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
                Name = "Test Org",
            };
            orgs.Add(newOrg);

            var repoData = new MemoryMetadata()
            {
                HasPassword = true,
                Password = "Pa$$w0rd",
                RootDirectory = @"C:\Users\alija\Documents\Programming\Test%20Project"
            };

            var newRepo = new Repository()
            {
                Name = "Test Repo",
                OrganizationId = newOrg.Id,
                SCM = SupportedSystems.Memory,
                CustomRepositoryInformation = JsonConvert.SerializeObject(repoData)
            };

            repos.Add(newRepo);

            var db = new DataContext(docs, orgs, repos);

            var queue = new MemoryQueue<RepositoryUpdatedMessage>();

            while (true)
            {
                queue.PushMessage(new RepositoryUpdatedMessage(newRepo.Id));

                var scraper = new Scraper(db);
                scraper.ScrapeRepository(queue.PopMessage());

                Display(db);
                Thread.Sleep(30000);
            }


        }

        private static void Display(DataContext ctx)
        {
            Console.WriteLine($"Documents at {DateTime.Now}:");
            foreach(var doc in ctx.Documents.GetWhere(x => true))
            {
                Console.WriteLine(JsonConvert.SerializeObject(doc));
            }
        }
    }
}
