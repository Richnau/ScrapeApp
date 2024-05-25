namespace ScrapeApp.Models;
public class ScrapablePage(string url)
{
	public string Url { get; set; } = url;
	public bool Scraped { get; set; } = false;
}
