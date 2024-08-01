using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobScraper.Models.TheProtocol
{
    public class TheProtocolOfferDescription
    {
        public List<string> Responsibilities { get; set; }

        public List<string> Technologies { get; set; }

        public List<string> Requirements { get; set; }

        public List<string> Offered { get; set; }
        
        public List<string> Benefits { get; set; }
    }
}
