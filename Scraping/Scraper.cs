using HtmlAgilityPack;
using ScrapeApp.Extensions;
using System.Collections.Concurrent;
using System.Diagnostics;
namespace ScrapeApp.Scraping;

public class Scraper(string startingUrl, string targetFolder)
{
	private ConcurrentDictionary<string, bool> DownloadableFiles { get; set; } = [];
	private ConcurrentDictionary<string, bool> ScrapablePages { get; set; } = [];
	private string StartingUrl { get; set; } = startingUrl;
	private string TargetFolder { get; set; } = targetFolder;
	private readonly object progressUpdateLock = new();

	public void ProcessSite()
	{
		Console.WriteLine();
		Console.WriteLine($"Scraping site: '{StartingUrl}' to folder: '{TargetFolder}'");
		Stopwatch stopWatch = new();
		stopWatch.Start();
		ScrapablePages.Clear();
		DownloadableFiles.Clear();
		ScrapeSite();

		stopWatch.Stop();
		var elapsedTime = stopWatch.Elapsed;
		Console.WriteLine();
		PrintElapsedTime(elapsedTime);
	}

	private void ScrapeSite()
	{
		ScrapablePages.TryAdd(StartingUrl, false);

		while (ScrapablePages.Any(p => !p.Value))
		{
			var scrapeTasks = ScrapablePages.Where(p => !p.Value).Take(48) //TODO: configurable batch size
				.Select(p => ScrapePageAsync(p.Key)).ToArray();

			Task.WaitAll(scrapeTasks);
			UpdateProgress();
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
			var downloadTasks = links.Select(link =>
			{
				string linkUrl = link.GetAttributeValue("src", link.GetAttributeValue("href", ""));
				var fileUri = new Uri(pageUri, linkUrl);
				var filePath = Path.Combine([TargetFolder, .. fileUri.AbsolutePath.Split('/')]);

				if (DownloadableFiles.TryAdd(filePath, false))
				{
					return DownloadFileAsync(fileUri.AbsoluteUri, filePath);
				}
				else
				{
					return null;
				}
			}).ToArray();
		}
	}

	private async Task DownloadFileAsync(string fileUrl, string filePath)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(filePath));
		using var httpClient = new HttpClient();
		var fileBytes = await httpClient.GetByteArrayAsync(fileUrl);
		File.WriteAllBytes(filePath, fileBytes);
		DownloadableFiles[filePath] = true;
		UpdateProgress();
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
					UpdateProgress();
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

	private void UpdateProgress()
	{
		if (Monitor.TryEnter(progressUpdateLock))
		{
			try
			{
				Console.SetCursorPosition(0, Console.CursorTop);
				Console.Write($"Scraped {ScrapablePages.Count(p => p.Value)} of {ScrapablePages.Count} pages. " +
					$"Downloaded {DownloadableFiles.Count(p => p.Value)} of {DownloadableFiles.Count} files.");
			}
			finally
			{
				Monitor.Exit(progressUpdateLock);
			}
		}
	}

	private void PrintElapsedTime(TimeSpan elapsedTime)
	{
		string hoursText = elapsedTime.Hours > 0 ? $"{elapsedTime.Hours} hours " : string.Empty;
		string minutesText = elapsedTime.Minutes > 0 ? $"{elapsedTime.Minutes} minutes" : string.Empty;
		string secondsText = $"{elapsedTime.Seconds} seconds";

		string timeText = string.Join(" ", hoursText, minutesText, secondsText).Trim();

		Console.WriteLine($"Scraped {ScrapablePages.Count} pages in {timeText}.");
	}
}
