
namespace ScrapeApp.Helpers;
static class UserInputHelper
{
	public static string GetUrlFromUser()
	{
		var validUrl = false;
		var sanitizedUrl = string.Empty;
		while (!validUrl)
		{
			Console.WriteLine("Please input the URL you wish to scrape:");
			var inputUrl = Console.ReadLine() ?? string.Empty;
			sanitizedUrl = UrlHelper.SanitizeUrl(inputUrl);
			validUrl = UrlHelper.IsValidUrlAsync(sanitizedUrl).Result;
		}
		return sanitizedUrl;
	}

	public static string GetTargetFolderFromUser()
	{
		Console.WriteLine("Please input the target folder where you want to save the scraped data:");
		var targetFolder = Console.ReadLine() ?? string.Empty;

		//TODO: Validate folder path?
		Directory.CreateDirectory(targetFolder);
		return targetFolder;
	}
}
