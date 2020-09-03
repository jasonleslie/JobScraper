using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Text;
using JobdataScraper;
using JobScraper.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JobScraper
{
    // Example URL :> "https://www.seek.com.au/jobs-in-information-communication-technology/in-New-South-Wales-NSW?daterange=7"
    class Seek : Website
    {
        private readonly DBContext _context;

        private const string
            urlHost = "www.seek.com.au/",
            urlSchema = "https://",
            urlOccupationField = "jobs-in-information-communication-technology/",     //Only want to see jobs in the IT field
            urlDateParam = "?daterange=";

        private string urlDateParamVal = "2";                                       //Only want to see jobs over the last 'x' days ( overwritten by what is in the config table in the DB )    

        //Australian states we wish to see results for ( this gets overwritten by what is in the config table in the DB )
        public string[] StatePaths { get; set; } =
                                                     {
                                                     "in-New-South-Wales-NSW",
                                                     "in-South-Australia-SA",
                                                     "in-Northern-Territory-NT",
                                                     "in-Tasmania-TAS",
                                                     "in-Western-Australia-WA",
                                                     "in-Victoria-VIC",
                                                     "in-Queensland-QLD",
                                                     "in-Australian-Capital-Territory-ACT"
                                                     };

        //Cities we wish to exclude ( this gets overwritten by what is in the config table in the DB )
        public string[] ExcludedLocations { get; set; } = { "Sydney", "Brisbane", "Melbourne" };

        public Seek(DBContext context)
        {
            _context = context;
            jobs = new List<Jobdata>();

            //Fetch search config from DB table (urlDateParamVal, StatePaths, ExcludedLocations)
            var searchConfig = _context.SeekConfig.First();

            StatePaths = searchConfig.SearchStates;
            ExcludedLocations = searchConfig.ExcludeLocations;
            urlDateParamVal = searchConfig.DateRange.ToString();
        }


        //Kicks off the site scraping process and ensures scraping continues until nothing more to retrieve
        public async Task ScrapeSite()
        {

            foreach (var statePath in StatePaths)
            {
                bool hasMoreResults = true;
                int pageCount = 0;

                var state = ExtractState(statePath);

                while (hasMoreResults)
                {

                    var response = await RequestJobResults(statePath, ++pageCount);

                    var jobElements = ParseJobResults(response);

                    hasMoreResults = jobElements.Any();

                    if (hasMoreResults)
                        ExtractJobFromResults(jobElements, state);

                    //*****************************************************TESTING***********
                    //hasMoreResults = false;
                }

                if (jobs.Any())
                    insertJobs();

                jobs = new List<Jobdata>();
            }

        }

        // Extracts state code from url path e.g. 'in-Victoria-VIC' -> 'VIC'
        private string ExtractState(string statePath)
        {
            string state;

            int lastHyphen = statePath.LastIndexOf('-');

            state = statePath.Substring(lastHyphen + 1);

            return state;
        }


        //Requests job results from website
        private async Task<Stream> RequestJobResults(string location, int pageCount)
        {

            CancellationTokenSource cancellationToken = new CancellationTokenSource();

            var urlPage = "&page=" + pageCount;

            Console.WriteLine("Got Page {0} of results{1}", pageCount, Environment.NewLine);

            HttpClient httpClient = new HttpClient();
            HttpResponseMessage request = await httpClient.GetAsync(urlSchema + urlHost + urlOccupationField + location + urlDateParam + urlDateParamVal + urlPage);
            cancellationToken.Token.ThrowIfCancellationRequested();

            Stream response = await request.Content.ReadAsStreamAsync();
            cancellationToken.Token.ThrowIfCancellationRequested();

            return response;

        }

        //Parses the HTML and extracts the HTML elements which are for jobs
        private IEnumerable<IElement> ParseJobResults(Stream response)
        {
            HtmlParser parser = new HtmlParser();
            IHtmlDocument document = parser.ParseDocument(response);

            IEnumerable<IElement> jobElements = null;

            jobElements = document.All.Where(x => (x.LocalName == "article" && x.GetAttribute("data-automation").Contains("Job")));

            return jobElements;
        }


        // Extracts the needed job info from the HTML elements to create our job objects
        private void ExtractJobFromResults(IEnumerable<IElement> jobElements, string state)
        {
            foreach (var jobElement in jobElements)
            {
                var tempNode = jobElement.QuerySelector("h1");

                var title = tempNode.TextContent;

                var link = urlSchema + (urlHost + tempNode.FirstElementChild.GetAttribute("href")).Replace("//", "/");

                var location = jobElement.QuerySelector("strong").FirstElementChild.QuerySelector("a").TextContent;

                var description = jobElement.QuerySelectorAll("span").Where(x => x.HasAttribute("data-automation") && x.GetAttribute("data-automation").Equals("jobShortDescription")).Select(x => x.TextContent).First();

                if (JobAreaNotExcluded(location))
                {
                    AddJob(title, location, state, link, description);
                }
            }
        }


        //Adds job to our list
        private void AddJob(string title, string location, string state, string link, string description)
        {
            Jobdata job = new Jobdata(title, location, state, link, description);

            //Want to prevent adding the same job twice to the list - which is possible due to Seek design and/or humans/recruiters double posting
            if (!jobs.Where(x => x.JobCompare(job)).Any())
                jobs.Add(job);
        }


        // Inserts all jobs in our list into our DB table
        private void insertJobs()
        {

            foreach (var job in jobs)
            {
                _context.Add(job);
            }

            _context.SaveChanges();

        }


        //Checks to see if job is in an excluded area
        private bool JobAreaNotExcluded(string location)
        {
            return !ExcludedLocations.Contains(location);
        }

    }
}
