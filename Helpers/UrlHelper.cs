namespace ScrapeApp.Helpers;

public class UrlHelper
{
	public static string GetPath(string url)
	{
		var uri = new Uri(EnsureProtocolPrefix(url));
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

	private static string EnsureProtocolPrefix(string url)
	{
		return !url.StartsWith("http://") && !url.StartsWith("https://")
			? "https://" + url
			: url;
	}
}
