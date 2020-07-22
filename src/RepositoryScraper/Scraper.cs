using DomainObjects;
using Queueing.Messages;
using Storage;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryScraper
{
    public class Scraper
    {
        /// For phase 1:
        ///     - Get the update message from the queue
        ///     - Walk the repository that changed
        ///     - Check if the readmes are the same (hash them or something)
        ///     - Update changes in the DB

        private IDataContext db;


        public Scraper(IDataContext db)
        {
            this.db = db;
        }

        public void ScrapeRepository(RepositoryUpdatedMessage msg)
        {
            var repo = db.Repositories.GetById(msg.RepositoryId);

            var scm = SourceControlFactory.GetSourceControlAccessor(repo.SCM);
            var documents = scm.GetReadmes(repo);

            var documentPathLookup = db.Documents.GetWhere(x => x.RepositoryId == repo.Id)
                .ToDictionary(x => x.Path, x => x.Id);

            foreach(var document in documents)
            {
                // There can only be one README.md per directory, so I'm hoping we can count on this check to
                // always be true and keep things simple...
                //
                // ...but I'm positive that'll blow up in my face now that I've said it.
                // There is surely a more efficient way anyways, at least for other SCM APIs
                if (documentPathLookup.ContainsKey(document.Path))
                {
                    document.Id = documentPathLookup[document.Path];
                    db.Documents.Update(document);
                }
                else
                {
                    db.Documents.Add(document);
                }
            }
        }

    }
}
