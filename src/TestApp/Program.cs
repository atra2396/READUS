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
//        static async Task Main(string[] args)
//        {
//            try
//            {
//                var personalaccesstoken = "dqqom4g5tcepdzkqs3xlzaoach3l7iljvwd2cdvftcieydlullyq";

//                using (HttpClient client = new HttpClient())
//                {
//                    client.DefaultRequestHeaders.Accept.Add(
//                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

//                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
//                        Convert.ToBase64String(
//                            System.Text.ASCIIEncoding.ASCII.GetBytes($":{personalaccesstoken}")));

//                    ProjectsResponse projectsResponse = null;
//                    using (HttpResponseMessage response = await client.GetAsync(
//                                "https://dev.azure.com/zhekau/_apis/projects?api-version=5.1").ConfigureAwait(false))
//                    {
//                        response.EnsureSuccessStatusCode();
//                        string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
//                        projectsResponse = JsonConvert.DeserializeObject<ProjectsResponse>(responseBody);
//                    }

//                    List<Repository> repositories = new List<Repository>();

//                    foreach (var project in projectsResponse.Projects)
//                    {
//                        using (HttpResponseMessage response = await client.GetAsync(
//                                $"https://dev.azure.com/zhekau/{project.id}/_apis/git/repositories?api-version=5.1").ConfigureAwait(false))
//                        {
//                            response.EnsureSuccessStatusCode();

//                            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
//                            var repositoriesResponse = JsonConvert.DeserializeObject<RepositoriesResponse>(responseBody);
//                            repositories.AddRange(repositoriesResponse.Repositories);
//                        }
//                    }

//                    List<Test> ReadmeItems = new List<Test>();

//                    foreach (var repo in repositories)
//                    {
//                        try
//                        {
//                            using (HttpResponseMessage response = await client.GetAsync(
//$"https://dev.azure.com/zhekau/{repo.project.id}/_apis/git/repositories/{repo.id}/items?recursionLevel=full&includeContentMetadata=true&latestProcessedChange=true&includeLinks=true&api-version=5.1").ConfigureAwait(false))
//                            {
//                                response.EnsureSuccessStatusCode();

//                                string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
//                                var itemsResponse = JsonConvert.DeserializeObject<ItemsResponse>(responseBody);
//                                foreach (var item in itemsResponse.Items)
//                                {
//                                    if (item.path.EndsWith("readme.md", StringComparison.CurrentCultureIgnoreCase))
//                                    {
//                                        ReadmeItems.Add(new Test { Repo = repo, Item = item });
//                                    }
//                                }
//                            }
//                        }
//                        catch (Exception)
//                        {

//                        }
//                    }

//                    foreach (var item in ReadmeItems)
//                    {
//                        try
//                        {
//                            using (HttpClient client1 = new HttpClient())
//                            {
//                                client1.DefaultRequestHeaders.Accept.Add(
//                                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));

//                                client1.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
//                                    Convert.ToBase64String(
//                                        System.Text.ASCIIEncoding.ASCII.GetBytes($":{personalaccesstoken}")));

//                                using (var str = await client1.GetStreamAsync(item.Item.url))
//                                {
//                                    using (var fileStream = File.Create(@"C:\Users\zheka\Desktop\readme.md"))
//                                    {
//                                        str.CopyTo(fileStream);
//                                    }
//                                }
//                            }
                                
                            
//                        }
//                        catch (Exception ex)
//                        {

//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.ToString());
//            }
//        }

        //public class Test
        //{
        //    public Item Item { get; set; }
        //    public Repository Repo { get; set; }
        //}

        //public class ContentMetadata
        //{
        //    public string fileName { get; set; }
        //}
        //public class Item
        //{
        //    public string objectId { get; set; }
        //    public string gitObjectType { get; set; }
        //    public string commitId { get; set; }
        //    public string path { get; set; }
        //    public bool isFolder { get; set; }
        //    public ContentMetadata contentMetadata { get; set; }
        //    public string url { get; set; }
        //}

        //public class ItemsResponse
        //{
        //    public int count { get; set; }
        //    [JsonProperty("value")]
        //    public List<Item> Items { get; set; }
        //}

        //public class Repository
        //{
        //    public string id { get; set; }
        //    public string name { get; set; }
        //    public string url { get; set; }
        //    public Project project { get; set; }
        //    public string remoteUrl { get; set; }
        //    public string sshUrl { get; set; }
        //    public string webUrl { get; set; }
        //}

        //public class RepositoriesResponse
        //{
        //    [JsonProperty("value")]
        //    public List<Repository> Repositories { get; set; }
        //    public int count { get; set; }
        //}

        //public class Project
        //{
        //    public string id { get; set; }
        //    public string name { get; set; }
        //    public string url { get; set; }
        //    public string state { get; set; }
        //    public int revision { get; set; }
        //    public string visibility { get; set; }
        //    public DateTime lastUpdateTime { get; set; }
        //}

        //public class ProjectsResponse
        //{
        //    public int count { get; set; }
        //    [JsonProperty("value")]
        //    public List<Project> Projects { get; set; }
        //}

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
