namespace ScrapeApp.Helpers;

public class UrlHelper
{
	public static string GetPath(string url)
	{
		if (!url.StartsWith("http://") && !url.StartsWith("https://"))
		{
			url = "https://" + url;
		}
		var uri = new Uri(url);
		var urlPath = uri.GetLeftPart(UriPartial.Path);
		return urlPath;
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
