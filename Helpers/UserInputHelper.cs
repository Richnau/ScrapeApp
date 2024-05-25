namespace ScrapeApp.Helpers;
static class UserInputHelper
{
	public static string GetUrlFromUser()
	{
		var validUrl = false;
		var urlPath = string.Empty;
		while (!validUrl)
		{
			Console.WriteLine("Please input the URL you wish to scrape:");
			var inputUrl = Console.ReadLine() ?? string.Empty;
			urlPath = UrlHelper.GetPath(inputUrl);
			validUrl = UrlHelper.IsValidUrlAsync(urlPath)?.Result ?? false;
		}
		return urlPath;
	}

	public static string GetTargetFolderFromUser()
	{
		try
		{
			Console.WriteLine("Please input the target folder where you want to save the scraped data:");
			var targetFolder = Console.ReadLine() ?? string.Empty;
			if (!string.IsNullOrEmpty(targetFolder))
			{
				Directory.CreateDirectory(targetFolder);
			}
			return targetFolder;
		}
		catch (Exception e)
		{
			Console.WriteLine($"Error creating folder: {e.Message}");
			return string.Empty;
		}

	}
}
