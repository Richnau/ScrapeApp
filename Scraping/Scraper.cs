using HtmlAgilityPack;
using ScrapeApp.Models;
namespace ScrapeApp.Scraping;

public class Scraper(string startingUrl)
{
	public List<ScrapablePage> ScrapablePages { get; set; } = new List<ScrapablePage>();
	public string StartingUrl { get; set; } = startingUrl;

	public void ProcessSite(string targetFolder = null)
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

			Task.WhenAll(scrapeTasks).Wait();
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

			ExtractFilePaths(pageToScrape.Url);
			ExtractLinks(pageToScrape.Url, document);

			pageToScrape.Scraped = true;
		}
		catch (Exception e)
		{
			Console.WriteLine($"Error scraping page: {e.Message}");
		}
	}

	private void ExtractFilePaths(string pageUrl)
	{
		// TODO: Implement file extraction
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
}
