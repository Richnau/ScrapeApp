namespace ScrapeApp.Helpers;

public class UrlHelper
{
	public static string SanitizeUrl(string url)
	{
		if (!url.StartsWith("http://") && !url.StartsWith("https://"))
		{
			url = "https://" + url;
		}
		var uri = new Uri(url);
		var sanitizedUrl = uri.GetLeftPart(UriPartial.Path);
		return sanitizedUrl;
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
