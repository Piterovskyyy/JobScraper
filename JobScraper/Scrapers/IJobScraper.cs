using JobScraper.Models.TheProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    namespace JobScraper.Scrapers
    {
        public interface IJobScraper
        {
            Task<IEnumerable<JobOffer>> ScrapeJobOffersAsync();
        }
    }


