using System;

namespace ScrapeApp.Helpers;

public class UrlHelper
{
	public static string SanitizeUrl(string url)
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
