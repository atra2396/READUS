﻿using DomainObjects;
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
        private static HttpClient DefaultHttpClient = new HttpClient();

        private static HttpClient StreamHttpClient = new HttpClient();

        public async Task<IEnumerable<Document>> GetReadmes(Repository repository)
        {
            if (repository == null) throw new ArgumentException($"Received a null {typeof(Repository).Name} object as a parameter in {nameof(GetReadmes)} method.");

            var metadata = JsonConvert.DeserializeObject<AzureDevOpsMetadata>(repository.CustomRepositoryInformation);

            var orgProjects = await GetProjects(repository, metadata).ConfigureAwait(false);

            IEnumerable<AzureDevOpsRepository>[] repositoriesResult = await GetRepositories(repository, metadata, orgProjects).ConfigureAwait(false);
            IEnumerable<Item>[] readmeItems = await GetRepositoryReadmeItems(repository, metadata, repositoriesResult).ConfigureAwait(false);
            IEnumerable<Document>[] readmeDocuments = await GetReadmeDocuments(repository, metadata, readmeItems).ConfigureAwait(false);

            return readmeDocuments.SelectMany(x => x);
        }

        private async Task<IEnumerable<Document>[]> GetReadmeDocuments(Repository repository, AzureDevOpsMetadata metadata, IEnumerable<Item>[] readmeItems)
        {
            List<Task<IEnumerable<Document>>> DocumentsTasks = new List<Task<IEnumerable<Document>>>();
            foreach (var readmeItemCollection in readmeItems)
                DocumentsTasks.Add(GetReadmesFromItems(readmeItemCollection, repository.Id, metadata));

            var readmeDocuments = await Task.WhenAll(DocumentsTasks.ToArray()).ConfigureAwait(false);
            return readmeDocuments;
        }

        private async Task<IEnumerable<Item>[]> GetRepositoryReadmeItems(Repository repository, AzureDevOpsMetadata metadata, IEnumerable<AzureDevOpsRepository>[] repositoriesResult)
        {
            List<Task<IEnumerable<Item>>> RepositoryReadmeItemsTasks = new List<Task<IEnumerable<Item>>>();
            foreach (var repoResult in repositoriesResult)
                foreach (var repo in repoResult)
                    RepositoryReadmeItemsTasks.Add(GetReadmeItemsFromRepository(repository, repo, metadata));

            var readmeItems = await Task.WhenAll(RepositoryReadmeItemsTasks.ToArray()).ConfigureAwait(false);
            return readmeItems;
        }

        private async Task<IEnumerable<AzureDevOpsRepository>[]> GetRepositories(Repository repository, AzureDevOpsMetadata metadata, IEnumerable<AzureDevOpsProject> orgProjects)
        {
            List<Task<IEnumerable<AzureDevOpsRepository>>> GetRepositoriesTasks = new List<Task<IEnumerable<AzureDevOpsRepository>>>();
            foreach (var proj in orgProjects)
                GetRepositoriesTasks.Add(GetRepositoriesFromProject(repository, proj, metadata));

            var repositoriesResult = await Task.WhenAll(GetRepositoriesTasks.ToArray()).ConfigureAwait(false);
            return repositoriesResult;
        }

        private async Task<IEnumerable<AzureDevOpsProject>> GetProjects(Repository repository, AzureDevOpsMetadata metadata)
        {
            List<AzureDevOpsProject> projects = new List<AzureDevOpsProject>();
            try
            {
                HttpClient client = GetAzureDevopsHttpClient(metadata);

                var responseBody = await client.GetStringAsync($"https://dev.azure.com/{repository.Name}/_apis/projects?api-version=5.1").ConfigureAwait(false);
                var itemsResponse = JsonConvert.DeserializeObject<AzureDevOpsProjectsResponse>(responseBody);
                foreach (var project in itemsResponse.Projects)
                {
                    projects.Add(project);
                }
            }
            catch (Exception ex)
            {
                //TODO: handle exceptions
                Console.WriteLine(ex);
            }
            return projects;
        }

        private async Task<IEnumerable<AzureDevOpsRepository>> GetRepositoriesFromProject(Repository repository, AzureDevOpsProject project, AzureDevOpsMetadata metadata)
        {
            List<AzureDevOpsRepository> repositories = new List<AzureDevOpsRepository>();
            try
            {
                HttpClient client = GetAzureDevopsHttpClient(metadata);

                var responseBody = await client.GetStringAsync($"https://dev.azure.com/{repository.Name}/{project.name}/_apis/git/repositories?api-version=5.1").ConfigureAwait(false);
                var repositoriesResponse = JsonConvert.DeserializeObject<AzureDevOpsRepositoryResponse>(responseBody);
                foreach (var repo in repositoriesResponse.repositories)
                {
                    repositories.Add(repo);
                }
            }
            catch (Exception ex)
            {
                //TODO: handle exceptions
                Console.WriteLine(ex);
            }
            return repositories;
        }
        private async Task<IEnumerable<Item>> GetReadmeItemsFromRepository(Repository repository, AzureDevOpsRepository azureDevOpsRepo, AzureDevOpsMetadata metadata)
        {
            List<Item> readmeItems = new List<Item>();
            try
            {
                HttpClient client = GetAzureDevopsHttpClient(metadata);
                var responseBody = await client.GetStringAsync(
$"https://dev.azure.com/{repository.Name}/{azureDevOpsRepo.project.name}/_apis/git/repositories/{azureDevOpsRepo.name}/items?recursionLevel=full&includeContentMetadata=true&latestProcessedChange=true&includeLinks=true&api-version=5.1").ConfigureAwait(false);
                var itemsResponse = JsonConvert.DeserializeObject<ItemsResponse>(responseBody);
                foreach (var item in itemsResponse.Items)
                {
                    if (item.path.EndsWith("readme.md", StringComparison.CurrentCultureIgnoreCase))
                    {
                        readmeItems.Add(item);
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

            await Task.WhenAll(readmeTasks.ToArray()).ConfigureAwait(false);

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
                var client = GetStreamAzureDevopsHttpClient(metadata);

                using (var str = await client.GetStreamAsync(item.url).ConfigureAwait(false))
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
            catch (Exception ex)
            {
                //TODO: handle exceptions
                Console.WriteLine(ex);
                return null;
            }
        }

        private static HttpClient GetAzureDevopsHttpClient(AzureDevOpsMetadata metadata)
        {
            DefaultHttpClient.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            DefaultHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes($":{metadata.PersonalAccessToken}")));
            return DefaultHttpClient;
        }

        private static HttpClient GetStreamAzureDevopsHttpClient(AzureDevOpsMetadata metadata)
        {
            StreamHttpClient.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));

            StreamHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    System.Text.ASCIIEncoding.ASCII.GetBytes($":{metadata.PersonalAccessToken}")));

            return StreamHttpClient;
        }
    }
}