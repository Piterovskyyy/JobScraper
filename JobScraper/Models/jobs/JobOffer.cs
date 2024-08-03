using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace JobScraper.Models.TheProtocol
{
    public class JobOffer
    {
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public string WorkModes { get; set; }
        public string Salary { get; set; }
        public string PositionLevels { get; set; }
        public string OfferValidTo { get;set; }
        public string ComapnyImgUrl { get; set; }
        public string Location { get; set; }
        public JobOfferDescription Description { get; set; }
        public string AboutCompany { get; set; }
        public string Url { get; set; }
    }
}
