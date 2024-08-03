using System.Collections.Generic;
using System.Threading.Tasks;
using JobScraper.Models;
using JobScraper.Models.TheProtocol;
using JobScraper.Scrapers;

namespace JobScraper.Services
{
    public class JobService
    {
        private readonly IJobScraper _scraper;

        public JobService(IJobScraper scraper)
        {
            _scraper = scraper;
        }

        public async Task<IEnumerable<JobOffer>> GetJobOffersAsync()
        {
            return await _scraper.ScrapeJobOffersAsync();
        }

   
    }
}