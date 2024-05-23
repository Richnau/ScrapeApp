using HtmlAgilityPack;
using ScrapeApp.Models;
namespace ScrapeApp.Scraping
{
	public class Scraper
	{
		public List<ScrapableUrl> ScrapablePages { get; set; } = [];
		public void ProcessSite(string siteUrl, string targetFolder = null)
		{
			ScrapablePages.Clear();
			ScrapeSite(siteUrl);
		}

		private void ScrapeSite(string siteUrl)
		{
			HtmlWeb web = new();
			HtmlDocument document = web.Load(siteUrl);

			var links = document.DocumentNode.SelectNodes("//a[@href]");
			if (links != null)
			{
				foreach (var link in links)
				{
					string pageUrl = link.GetAttributeValue("href", "");
					string domainUrl = new Uri(siteUrl).GetLeftPart(UriPartial.Authority);
					string extractedUrl = new Uri(new Uri(domainUrl), pageUrl).AbsoluteUri;
					ScrapablePages.Add(new ScrapableUrl(pageUrl));
				}
			}
		}
	}
}
