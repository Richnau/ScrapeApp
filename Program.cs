using ScrapeApp.Helpers;
using ScrapeApp.Scraping;
namespace ScrapeApp;

class Program
{
	static void Main()
	{
		Console.WriteLine("Welcome to the Scrape App! (Press Ctrl-C to exit at any time.)");
		var finishedScraping = false;
		while (!finishedScraping)
		{
			var siteUrl = UserInputHelper.GetUrlFromUser();
			var targetFolder = UserInputHelper.GetTargetFolderFromUser();

			var scraper = new Scraper();
			scraper.ProcessSite(siteUrl);

			Console.WriteLine("Would you like to scrape another URL? (Y/N)");
			var input = Console.ReadLine();
			finishedScraping = input?.Equals("y", StringComparison.OrdinalIgnoreCase) == true
					|| input?.Equals("yes", StringComparison.OrdinalIgnoreCase) == true;
		}
	}
}
