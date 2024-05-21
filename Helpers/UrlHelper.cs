    class UrlHelper
    {
        public static string GetUrlFromUser()
        {
            Console.WriteLine("Please input the URL you wish to scrape:");
            var inputUrl = Console.ReadLine() ?? string.Empty;
            var sanitizedUrl = SanitizeUrl(inputUrl);
            // TODO: implement validation
            return sanitizedUrl;
        }

        private static string SanitizeUrl(string url)
        {
            // TODO: Implement sanitation. Drop querystring etc.
            return url;
        }
    }