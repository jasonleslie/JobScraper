using JobScraper.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace JobScraper
{
    class Program
    {

        private static Seek _seekService;

        static async Task Main(string[] args)
        {

            var dbConnectString = ConfigurationManager.ConnectionStrings["postgres" + Environment.GetEnvironmentVariable("JOBSEEKER_ENV")].ConnectionString;

            var serviceProvider = new ServiceCollection()
            .AddSingleton<Seek>()
            .AddDbContext<DBContext>(options => options.UseNpgsql(dbConnectString))
            .BuildServiceProvider();

            _seekService = serviceProvider.GetService<Seek>();

            await ScrapeSites();

        }

        //Scrape all Sites (only Seek at the moment)
        public static async Task ScrapeSites()
        {

            await _seekService.ScrapeSite();

        }
    }
}
