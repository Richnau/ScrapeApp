using ScrapeApp.Helpers;
using ScrapeApp.Scraping;
namespace ScrapeApp;

class Program
{
	static void Main()
	{
		Console.WriteLine("Welcome to the Scrape App! (Press Ctrl-C to exit at any time.)");
		var keepScraping = true;
		while (keepScraping)
		{
			var siteUrl = UserInputHelper.GetUrl();
			var targetFolder = UserInputHelper.GetOrCreateTargetFolder();

			var scraper = new Scraper(siteUrl, targetFolder);
			scraper.ProcessSite();

			Console.WriteLine("Would you like to scrape another URL? (Y/N)");
			var input = Console.ReadLine() ?? string.Empty;
			keepScraping = input.Equals("y", StringComparison.OrdinalIgnoreCase)
					|| input.Equals("yes", StringComparison.OrdinalIgnoreCase);
		}
		Console.WriteLine("Thank you for using the Scrape App!");
		Console.ReadKey();
	}
}
