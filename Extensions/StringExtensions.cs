namespace ScrapeApp.Extensions
{
	public static class StringExtensions
	{
		public static string OrIfNullOrEmpty(this string value, string defaultValue)
		{
			return string.IsNullOrEmpty(value) ? defaultValue : value;
		}
	}
}
