using DomainObjects;
using Newtonsoft.Json;
using SourceControl.AzureDevOps.POCOs;
using System;
using System.Collections.Generic;
using System.IO;
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
            var readmeItems = await GetReadmeItems(repository, metadata);

            var readmes = await GetReadmesFromItems(readmeItems, repository.Id, metadata);

            return readmes;
        }

        private async Task<IEnumerable<Item>> GetReadmeItems(Repository repository, AzureDevOpsMetadata metadata)
        {
            List<Item> readmeItems = new List<Item>();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                        Convert.ToBase64String(
                            System.Text.ASCIIEncoding.ASCII.GetBytes($":{metadata.PersonalAccessToken}")));
                    using (HttpResponseMessage response = await client.GetAsync(
$"https://dev.azure.com/{repository.OrganizationName}/{repository.ProjectName}/_apis/git/repositories/{repository.Name}/items?recursionLevel=full&includeContentMetadata=true&latestProcessedChange=true&includeLinks=true&api-version=5.1").ConfigureAwait(false))
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
    }
}