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
            //IJobScraper scraper = new TheProtocolScraper();
            //JobService jobService = new JobService(scraper);

            //var jobOffers = await jobService.GetJobOffersAsync();
            //Console.WriteLine(jobOffers.Count());


            IJobScraper scraper = new PracujScraper();
            JobService jobService = new JobService(scraper);
            var jobOffers = await jobService.GetJobOffersAsync();
            Console.WriteLine(jobOffers.Count());
            foreach (var offer in jobOffers)
            {
                Console.WriteLine($"Title: {offer.Title}");
                Console.WriteLine($"Company: {offer.CompanyName}");
                Console.WriteLine($"Location: {offer.Location}");
                Console.WriteLine($"Work Modes: {offer.WorkModes}");
                Console.WriteLine($"Position Levels: {offer.PositionLevels}");
                Console.WriteLine($"Salary: {offer.Salary}");
                Console.WriteLine($"Offer Valid To: {offer.OfferValidTo}");
                Console.WriteLine($"Company Image URL: {offer.ComapnyImgUrl}");
                Console.WriteLine($"About Company: {offer.AboutCompany}");
                Console.WriteLine($"URL: {offer.Url}");
                Console.WriteLine("Responsibilities:");
                foreach (var responsibility in offer.Description.Responsibilities)
                {
                    Console.WriteLine($"- {responsibility}");
                }
                Console.WriteLine("Requirements:");
                foreach (var requirement in offer.Description.Requirements)
                {
                    Console.WriteLine($"- {requirement}");
                }
                Console.WriteLine("Offered:");
                foreach (var offered in offer.Description.Offered)
                {
                    Console.WriteLine($"- {offered}");
                }
                Console.WriteLine("Benefits:");
                foreach (var benefit in offer.Description.Benefits)
                {
                    Console.WriteLine($"- {benefit}");
                }
                Console.WriteLine(new string('-', 80));
            }
        }
    }
}
