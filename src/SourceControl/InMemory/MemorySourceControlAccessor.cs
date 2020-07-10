using DomainObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SourceControl.InMemory
{
    public class MemorySourceControlAccessor : ISourceControlAccessor
    {
        public MemorySourceControlAccessor()
        {

        }

        public IEnumerable<Document> GetReadmes(Repository repository)
        {
            var metadata =  JsonConvert.DeserializeObject<MemoryMetadata>(repository.CustomRepositoryInformation);

            // Probably an API key or something IRL
            if (metadata.HasPassword)
            {
                Console.WriteLine($"shh, the password is {metadata.Password}");
            }

            return GetDocumentsByFolder(metadata.RootDirectory, repository.Id);
        }

        IEnumerable<Document> GetDocumentsByFolder(string path, Guid repoId)
        {
            // README name will be from somewhere else, maybe a field within the Repository object
            // also, this does not take into account images yet
            var files = Directory.GetFiles(path);
            if (files.Contains(Path.Combine(path, "README.md")))
            {
                var text = File.ReadAllText(Path.Combine(path, "README.md"));
                yield return new Document
                {
                    Path = path,
                    Body = text,
                    Created = DateTime.UtcNow,  // don't feel like reading metadata
                    RepositoryId = repoId,
                    Updated = DateTime.UtcNow
                };
            }

            // Eventually, should give you user a chance to ignore useless directories (like .git)
            foreach(var directory in Directory.GetDirectories(path))
            {
                foreach (var document in GetDocumentsByFolder(directory, repoId))
                    yield return document;
            }

        }
    }
}
