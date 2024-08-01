using System;
using System.Threading.Tasks;
using JobScraper.Services;
using JobScraper.Scrapers;

namespace JobScraper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IJobScraper scraper = new TheProtocolScraper();
            JobService jobService = new JobService(scraper);

            var jobOffers = await jobService.GetJobOffersAsync();
            Console.WriteLine(jobOffers.Count());

           

        }
    }
}
