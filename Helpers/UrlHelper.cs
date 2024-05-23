namespace ScrapeApp.Helpers;
static class UrlHelper
{
	public static string GetUrlFromUser()
	{
		var validUrl = false;
		var sanitizedUrl = string.Empty;
		while (!validUrl)
		{
			Console.WriteLine("Please input the URL you wish to scrape:");
			var inputUrl = Console.ReadLine() ?? string.Empty;
			sanitizedUrl = SanitizeUrl(inputUrl);
			validUrl = IsValidUrlAsync(sanitizedUrl).Result;
		}
		return sanitizedUrl;
	}

	private static string SanitizeUrl(string url)
	{
		// TODO: Implement sanitation. Drop querystring etc.
		return url;
	}

	public static async Task<bool> IsValidUrlAsync(string url)
	{
		using var httpClient = new HttpClient();
		try
		{
			var response = await httpClient.GetAsync(url);
			return response.IsSuccessStatusCode;
		}
		catch (Exception e)
		{
			Console.WriteLine($"Error validating URL: {e.Message}");
			return false;
		}
	}
}