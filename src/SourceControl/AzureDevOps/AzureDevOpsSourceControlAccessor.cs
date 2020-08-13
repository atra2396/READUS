using DomainObjects;
using Newtonsoft.Json;
using SourceControl.AzureDevOps.POCOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SourceControl.AzureDevOps
{
    public class AzureDevOpsSourceControlAccessor : ISourceControlAccessor
    {
        public async Task<IEnumerable<Document>> GetReadmes(Repository repository)
        {
            if (repository == null) throw new ArgumentException($"Received a null {typeof(Repository).Name} object as a parameter in {nameof(GetReadmes)} method.");

            var metadata = JsonConvert.DeserializeObject<AzureDevOpsMetadata>(repository.CustomRepositoryInformation);

            var orgProjects = await GetProjects(repository, metadata);
            List<Task<IEnumerable<AzureDevOpsRepository>>> GetRepositoriesTasks = new List<Task<IEnumerable<AzureDevOpsRepository>>>();

            foreach (var proj in orgProjects)
                GetRepositoriesTasks.Add(GetRepositoriesFromProject(repository, proj, metadata.PersonalAccessToken));

            var repositoriesResult = await Task.WhenAll(GetRepositoriesTasks.ToArray());

            List<Task<IEnumerable<Item>>> ReadmeItemsTasks = new List<Task<IEnumerable<Item>>>();
            foreach (var repoResult in repositoriesResult)
                foreach (var repo in repoResult)
                    ReadmeItemsTasks.Add(GetReadmeItemsFromRepository(repository, repo, metadata));

            var readmeItems = await Task.WhenAll(ReadmeItemsTasks.ToArray());

            List<Task<IEnumerable<Document>>> DocumentsTasks = new List<Task<IEnumerable<Document>>>();
            foreach (var readmeItemCollection in readmeItems)
                DocumentsTasks.Add(GetReadmesFromItems(readmeItemCollection, repository.Id, metadata));

            var readmeDocuments = await Task.WhenAll(DocumentsTasks.ToArray());

            return readmeDocuments.SelectMany(x => x);
        }

        private async Task<IEnumerable<Project>> GetProjects(Repository repository, AzureDevOpsMetadata metadata)
        {
            List<Project> projects = new List<Project>();
            try
            {
                var client = GetAzureDevopsHttpClient(metadata.PersonalAccessToken);

                using (HttpResponseMessage response = await client.GetAsync($"https://dev.azure.com/{repository.Name}/_apis/projects?api-version=5.1"))
                {
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    var itemsResponse = JsonConvert.DeserializeObject<ProjectsResponse>(responseBody);
                    foreach (var project in itemsResponse.Projects)
                    {
                        projects.Add(project);
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: handle exceptions
                Console.WriteLine(ex);
            }
            return projects;
        }

        private async Task<IEnumerable<AzureDevOpsRepository>> GetRepositoriesFromProject(Repository repository, Project project, string personalAccessToken)
        {
            List<AzureDevOpsRepository> repositories = new List<AzureDevOpsRepository>();
            try
            {
                var client = GetAzureDevopsHttpClient(personalAccessToken);

                using (HttpResponseMessage response = await client.GetAsync($"https://dev.azure.com/{repository.Name}/{project.name}/_apis/git/repositories?api-version=5.1"))
                {
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    var repositoriesResponse = JsonConvert.DeserializeObject<AzureDevOpsRepositoryResponse>(responseBody);
                    foreach (var repo in repositoriesResponse.repositories)
                    {
                        repositories.Add(repo);
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: handle exceptions
                Console.WriteLine(ex);
            }
            return repositories;
        }

        private static HttpClient GetAzureDevopsHttpClient(string personalAccessToken)
        {
            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes($":{personalAccessToken}")));
            return httpClient;
        }

        private async Task<IEnumerable<Item>> GetReadmeItemsFromRepository(Repository repository, AzureDevOpsRepository azureDevOpsRepo, AzureDevOpsMetadata metadata)
        {
            List<Item> readmeItems = new List<Item>();
            try
            {
                var client = GetAzureDevopsHttpClient(metadata.PersonalAccessToken);
                using (HttpResponseMessage response = await client.GetAsync(
$"https://dev.azure.com/{repository.Name}/{azureDevOpsRepo.project.name}/_apis/git/repositories/{azureDevOpsRepo.name}/items?recursionLevel=full&includeContentMetadata=true&latestProcessedChange=true&includeLinks=true&api-version=5.1").ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var itemsResponse = JsonConvert.DeserializeObject<ItemsResponse>(responseBody);
                    foreach (var item in itemsResponse.Items)
                    {
                        if (item.path.EndsWith("readme.md", StringComparison.CurrentCultureIgnoreCase))
                        {
                            readmeItems.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: handle exceptions
                Console.WriteLine(ex);
            }
            return readmeItems;
        }


        private async Task<IEnumerable<Document>> GetReadmesFromItems(IEnumerable<Item> readmeItems, Guid repositoryId, AzureDevOpsMetadata metadata)
        {
            List<Task<Document>> readmeTasks = new List<Task<Document>>();

            foreach (var item in readmeItems)
            {
                readmeTasks.Add(GetReadmeDocumentFromURL(item, repositoryId, metadata));
            }

            await Task.WhenAll(readmeTasks.ToArray());

            List<Document> readmeDocuments = new List<Document>();
            foreach (var task in readmeTasks)
            {
                readmeDocuments.Add(task.Result);
            }
            return readmeDocuments;
        }

        private async Task<Document> GetReadmeDocumentFromURL(Item item, Guid repositoryId, AzureDevOpsMetadata metadata)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes($":{metadata.PersonalAccessToken}")));

                    using (var str = await client.GetStreamAsync(item.url))
                    {
                        using (StreamReader reader = new StreamReader(str))
                        {
                            var readmeContent = reader.ReadToEnd();
                            return new Document
                            {
                                Body = readmeContent,
                                RepositoryId = repositoryId,
                                Path = item.path,
                                //TODO: Get these from AzureDevOps API response
                                Created = DateTime.UtcNow,
                                Updated = DateTime.UtcNow
                            };

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: handle exceptions
                Console.WriteLine(ex);
                return null;
            }
        }

        public class Project
        {
            public string id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public string state { get; set; }
            public int revision { get; set; }
            public string visibility { get; set; }
            public DateTime lastUpdateTime { get; set; }
        }

        public class ProjectsResponse
        {
            public int count { get; set; }
            [JsonProperty("value")]
            public List<Project> Projects { get; set; }
        }

        public class AzureDevOpsRepository
        {
            public string id { get; set; }
            public string name { get; set; }
            public string url { get; set; }
            public Project project { get; set; }
            public string remoteUrl { get; set; }
            public string sshUrl { get; set; }
            public string webUrl { get; set; }
        }

        public class AzureDevOpsRepositoryResponse
        {
            [JsonProperty("value")]
            public List<AzureDevOpsRepository> repositories { get; set; }
            public int count { get; set; }
        }
    }
}