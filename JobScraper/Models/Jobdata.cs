using System;

namespace JobScraper.Models
{
    public partial class Jobdata
    {


        public Jobdata(string title, string location, string state, string link, string description)
        {
            Title = title;
            Location = location;
            State = state;
            Link = link;
            Description = description;
        }

        public long Id { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public string State { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
        public DateTime? Createdate { get; set; }

        //Used to check if two jobs are the same or not
        public bool JobCompare(Jobdata job)
        {
            return ((this.Title == job.Title) && (this.Location == job.Location) && (this.Description == job.Description));
        }
    }
}
