using HtmlAgilityPack;
using ScrapeApp.Extensions;
using System.Collections.Concurrent;
namespace ScrapeApp.Scraping;

public class Scraper(string startingUrl, string targetFolder)
{
	private ConcurrentDictionary<string, bool> DownloadableFiles { get; set; } = [];

	private ConcurrentDictionary<string, bool> ScrapablePages { get; set; } = [];
	private string StartingUrl { get; set; } = startingUrl;
	private string TargetFolder { get; set; } = targetFolder;

	public void ProcessSite()
	{
		ScrapablePages.Clear();
		ScrapeSite();
		foreach (var page in ScrapablePages)
		{
			Console.WriteLine($"Scraped page: {page.Key}");
		}
	}

	private void ScrapeSite()
	{
		ScrapablePages.TryAdd(StartingUrl, false);

		while (ScrapablePages.Any(p => !p.Value))
		{
			var scrapeTasks = ScrapablePages.Where(p => !p.Value).Take(48) //TODO: configurable batch size
				.Select(p => ScrapePageAsync(p.Key)).ToList();

			Task.WhenAll(scrapeTasks).Wait(); //TODO: add minimum wait time to avoid request spamming?
		}
	}

	private async Task ScrapePageAsync(string pageUrl)
	{
		try
		{
			using var httpClient = new HttpClient();
			var html = await httpClient.GetStringAsync(pageUrl);
			var document = new HtmlDocument();
			document.LoadHtml(html);
			DownloadFiles(pageUrl, document);
			ExtractLinks(pageUrl, document);

			ScrapablePages[pageUrl] = true;
		}
		catch (Exception e)
		{
			Console.WriteLine($"Error scraping page: {e.Message}");
		}
	}

	private void DownloadFiles(string pageUrl, HtmlDocument document)
	{
		var pageName = Path.GetFileName(pageUrl);
		var pageUri = new Uri(pageUrl);
		var pagePath = Path.Combine(
			[TargetFolder,
			Path.GetDirectoryName(pageUri.AbsolutePath)?.TrimStart('\\') ?? string.Empty,
			pageName.OrIfNullOrEmpty("index.html")]);

		if (DownloadableFiles.TryAdd(pagePath, false))
		{
			Directory.CreateDirectory(Path.GetDirectoryName(pagePath));
			document.Save(pagePath);
			DownloadableFiles[pagePath] = true;
		}

		var links = GetFileLinks(document);

		if (links != null)
		{
			foreach (var link in links)
			{
				string linkUrl = link.GetAttributeValue("src", link.GetAttributeValue("href", ""));
				var linkUri = new Uri(pageUri, linkUrl);
				var filePath = Path.Combine([TargetFolder, .. linkUri.AbsolutePath.Split('/')]);

				if (DownloadableFiles.TryAdd(filePath, false)){
					Directory.CreateDirectory(Path.GetDirectoryName(filePath));
					using var httpClient = new HttpClient();
					var fileBytes = httpClient.GetByteArrayAsync(linkUri).Result;
					File.WriteAllBytes(filePath, fileBytes);
				}
			}
		}
	}

	private void ExtractLinks(string pageUrl, HtmlDocument document)
	{
		var links = document.DocumentNode.SelectNodes("//a[@href]");
		if (links != null)
		{
			var baseUrl = new Uri(pageUrl);
			foreach (var link in links)
			{
				string linkUrl = link.GetAttributeValue("href", "");
				var uri = new Uri(baseUrl, linkUrl);
				if (uri.AbsoluteUri.StartsWith(StartingUrl))
				{
					ScrapablePages.TryAdd(uri.AbsoluteUri, false);
				}
			}
		}
	}

	private static List<HtmlNode> GetFileLinks(HtmlDocument document)
	{
		var links = new List<HtmlNode>();
		links.AddRange(document.DocumentNode.SelectNodes("//img[@src]")?.ToList() ?? []);
		links.AddRange(document.DocumentNode.SelectNodes("//script[@src]")?.ToList() ?? []);
		links.AddRange(document.DocumentNode.SelectNodes("//link[@href][@rel='stylesheet']")?.ToList() ?? []);
		links.AddRange(document.DocumentNode.SelectNodes("//link[@href][@rel='icon']")?.ToList() ?? []);
		links.AddRange(document.DocumentNode.SelectNodes("//link[@href][@rel='shortcut icon']")?.ToList() ?? []);
		return links;
	}
}
