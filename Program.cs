namespace ScrapeApp;
using ScrapeApp.Helpers;

class Program
{
	static void Main()
	{
		bool keepAlive = true;
		Console.WriteLine("Welcome to the Scrape App! (Press Ctrl-C to exit at any time.)");
		while (keepAlive)
		{
			var url = UrlHelper.GetUrlFromUser();
			// TODO: Use the url for scraping

			Console.WriteLine("Would you like to scrape another URL? (Y/N)");
			var input = Console.ReadLine();
			keepAlive = input?.Equals("y", StringComparison.OrdinalIgnoreCase) == true
					|| input?.Equals("yes", StringComparison.OrdinalIgnoreCase) == true;
		}
	}
}