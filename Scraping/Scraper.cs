using HtmlAgilityPack;
using ScrapeApp.Extensions;
using ScrapeApp.Models;
namespace ScrapeApp.Scraping;

public class Scraper(string startingUrl, string targetFolder)
{
	public List<ScrapablePage> ScrapablePages { get; set; } = [];
	public string StartingUrl { get; set; } = startingUrl;
	public string TargetFolder { get; set; } = targetFolder;

	public void ProcessSite()
	{
		ScrapablePages.Clear();
		ScrapeSite();
		foreach (var page in ScrapablePages)
		{
			Console.WriteLine($"Scraped page: {page.Url}");
		}
	}

	private void ScrapeSite()
	{
		ScrapablePages.Add(new ScrapablePage(StartingUrl));

		while (ScrapablePages.Any(x => !x.Scraped))
		{
			var scrapeTasks = ScrapablePages.Where(p => !p.Scraped).Take(1) //TODO: configurable batch size
				.Select(ScrapePageAsync).ToList();

			Task.WhenAll(scrapeTasks).Wait(); //TODO: add minimum wait time to avoid request spamming?
		}
	}

	private async Task ScrapePageAsync(ScrapablePage pageToScrape)
	{
		try
		{
			using var httpClient = new HttpClient();
			var html = await httpClient.GetStringAsync(pageToScrape.Url);
			var document = new HtmlDocument();
			document.LoadHtml(html);
			DownloadFiles(pageToScrape.Url, document);
			ExtractLinks(pageToScrape.Url, document);

			pageToScrape.Scraped = true;
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

		if (!File.Exists(pagePath)){
			Directory.CreateDirectory(Path.GetDirectoryName(pagePath));
			document.Save(pagePath);
		}

		var links = GetFileLinks(document);

		if (links != null)
		{
			foreach (var link in links)
			{
				string linkUrl = link.GetAttributeValue("src", link.GetAttributeValue("href", ""));
				var linkUri = new Uri(pageUri, linkUrl);
				var filePath = Path.Combine([TargetFolder, .. linkUri.AbsolutePath.Split('/')]);
				Directory.CreateDirectory(Path.GetDirectoryName(filePath));

				if (!File.Exists(filePath))
				{
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
				if (uri.AbsoluteUri.StartsWith(StartingUrl) && !ScrapablePages.Any(x => x.Url == uri.AbsoluteUri))
				{
					ScrapablePages.Add(new ScrapablePage(uri.AbsoluteUri));
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
