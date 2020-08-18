using DomainObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SourceControl.GitHub
{
    public class GitHubSourceControlAccessor : ISourceControlAccessor
    {
        public async Task<IEnumerable<Document>> GetReadmes(Repository repository)
        {
            if (repository == null) throw new ArgumentException($"Received a null {typeof(Repository).Name} object as a parameter in {nameof(GetReadmes)} method.");

            var metadata = JsonConvert.DeserializeObject<GitHubMetadata>(repository.CustomRepositoryInformation);
            var readmeItems = await GetReadmeItems(repository, metadata);

            var readmes = await GetReadmesFromItems(readmeItems, repository.Id, metadata);

            return readmes;
        }
        private async Task<IEnumerable<Item>> GetReadmeItems(Repository repository, GitHubMetadata metadata)
        {
            List<Item> readmeItems = new List<Item>();
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("product", "1"));
                    client.DefaultRequestHeaders.Add("Authorization", $"token {metadata.PersonalAccessToken}");
                    using (HttpResponseMessage response = await client.GetAsync(
$"https://api.github.com/search/code?q=org:{repository.Name}+filename:readme").ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();

                        string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var itemsResponse = JsonConvert.DeserializeObject<ItemsResponse>(responseBody);
                        foreach (var item in itemsResponse.items)
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


        private async Task<IEnumerable<Document>> GetReadmesFromItems(IEnumerable<Item> readmeItems, Guid repositoryId, GitHubMetadata metadata)
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

        private async Task<Document> GetReadmeDocumentFromURL(Item item, Guid repositoryId, GitHubMetadata metadata)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("product", "1"));

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization",
                         $"token {metadata.PersonalAccessToken}");

                    using (HttpResponseMessage response = await client.GetAsync($"{item.url}"))
                    {
                        response.EnsureSuccessStatusCode();

                        string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var readmeResponse = JsonConvert.DeserializeObject<ReadmeResponse>(responseBody);

                        return DecodeReadme(readmeResponse, repositoryId);
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

        private Document DecodeReadme(ReadmeResponse readmeResponse, Guid repositoryId)
        {
            switch (readmeResponse.encoding)
            {
                case "base64":
                    byte[] data = Convert.FromBase64String(readmeResponse.content);
                    string decodedString = Encoding.UTF8.GetString(data);
                    return new Document 
                    {
                        Body = decodedString,
                        RepositoryId = repositoryId,
                        Path = readmeResponse.path,
                        Created = DateTime.UtcNow,
                        Updated = DateTime.UtcNow
                    };
                default:
                    throw new NotImplementedException($"Decoding for enconding type {readmeResponse.encoding} hasn't been implemented");
            }
        }
    }

    public class ReadmeResponse
    {
        public string path { get; set; }
        public string type { get; set; }
        public string content { get; set; }
        public string encoding { get; set; }
    }

    public class GitHubRepository
    {
        public int id { get; set; }
        public string description { get; set; }
        public bool fork { get; set; }
    }

    public class Item
    {
        public string name { get; set; }
        public string path { get; set; }
        public string sha { get; set; }
        public string url { get; set; }
        public GitHubRepository repository { get; set; }
        public double score { get; set; }
    }

    public class ItemsResponse
    {
        public int total_count { get; set; }
        public bool incomplete_results { get; set; }
        public List<Item> items { get; set; }
    }
}
