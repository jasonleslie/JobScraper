using JobScraper.Models;
using System.Collections.Generic;

namespace JobdataScraper
{
    class Website
    {
        public List<Jobdata> jobs { get; set; }
        public Website()
        {
            jobs = new List<Jobdata>();
        }
    }
}
