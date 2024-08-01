using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace JobScraper.Scrapers
{
    public abstract class BaseScraper
    {
        virtual protected async Task<string> GetHtmlAsync(string url)
        {
            using var client = new HttpClient();
            return await client.GetStringAsync(url);
        }

        protected HtmlDocument LoadHtml(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return document;
        }
    }
}