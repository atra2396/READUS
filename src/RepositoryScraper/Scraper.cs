using DomainObjects;
using Queueing.Messages;
using Storage;
using System;
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

        private DataContext db;
        // private SourceControl scm;


        public Scraper(DataContext db)
        {
            this.db = db;
        }

        public void ScrapeRepository(RepositoryUpdatedMessage msg)
        {
        }

    }
}
