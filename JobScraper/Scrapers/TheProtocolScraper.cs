using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using HtmlAgilityPack;
using JobScraper.Models;
using JobScraper.Models.TheProtocol;
using static System.Collections.Specialized.BitVector32;

namespace JobScraper.Scrapers
{
    public class TheProtocolScraper : BaseScraper, IJobScraper
    {
        private static readonly HttpClient _client = new HttpClient();
        private readonly string _url = "https://theprotocol.it/filtry/trainee,assistant,junior;p";
        private int _countOffers = 0;
        private int _pageNumber = 1;
        private readonly HashSet<string> _visitedUrls = new HashSet<string>();

        static TheProtocolScraper()
        {
            _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));
            _client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
        }

        public async Task<IEnumerable<JobOffer>> ScrapeJobOffersAsync()
        {
            var jobOffers = new List<JobOffer>();
            string currentUrl = _url;

            try
            {
                while (!string.IsNullOrEmpty(currentUrl))
                {
                    var html = await GetHtmlAsync(currentUrl);
                    var document = LoadHtml(html);

                    var offersSection = document.DocumentNode.SelectSingleNode("//div[@data-test='offersList']");
                    if (offersSection == null) break;

                    var offerNodes = offersSection.SelectNodes(".//a[contains(@class, 'a4pzt2q')]");
                    if (offerNodes == null) break;

                    var offerTasks = new List<Task<JobOffer>>();

                    foreach (var offerNode in offerNodes)
                    {
                        var offerUrl = offerNode.GetAttributeValue("href", string.Empty);
                        var fullOfferUrl = new Uri(new Uri(_url), offerUrl).ToString();

                        if (!_visitedUrls.Contains(fullOfferUrl))
                        {
                            _visitedUrls.Add(fullOfferUrl);
                            offerTasks.Add(ProcessOfferAsync(fullOfferUrl));
                           
                        }
                       
                    }
                   
                    

                    var offers = await Task.WhenAll(offerTasks);
                    jobOffers.AddRange(offers);
                     var nextPageNode = document.DocumentNode.SelectSingleNode("//a[@data-test='anchor-nextPage']");
                     _pageNumber++;
                   
                    currentUrl = nextPageNode != null
                        ? new Uri(new Uri(_url), $"?pageNumber={_pageNumber}").ToString()
                        : null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas skrapowania: {ex.Message}");
            }

            return jobOffers;
        }

        private async Task<JobOffer> ProcessOfferAsync(string url)
        {
         

            var offerHtml = await GetHtmlAsync(url);
            var offerDocument = LoadHtml(offerHtml);
            var description = GetTheProtocolOfferDescription(offerDocument);
            _countOffers++;
            Console.WriteLine($"Ilosc przetworznych ofert: {_countOffers}");
            Console.WriteLine($"Przetworzono ofertę: {offerDocument.DocumentNode.SelectSingleNode("//h1[@data-test='text-offerTitle']")?.InnerText.Trim()}");

            return new JobOffer
            {
                Title = offerDocument.DocumentNode.SelectSingleNode("//h1[@data-test='text-offerTitle']")?.InnerText.Trim() ?? "Brak danych",
                CompanyName = offerDocument.DocumentNode.SelectSingleNode("//a[@data-test='anchor-company-link']")?.InnerText.Trim() ?? "Brak danych",
                WorkModes = offerDocument.DocumentNode.SelectSingleNode("//div[@data-test='section-workModes']")?.InnerText.Trim() ?? "Brak danych",
                PositionLevels = offerDocument.DocumentNode.SelectSingleNode("//div[@data-test='section-positionLevels']")?.InnerText.Trim() ?? "Brak danych",
                Location = offerDocument.DocumentNode.SelectSingleNode("//div[@data-test='section-workplace']")?.InnerText.Trim() ?? "Brak danych",
                OfferValidTo = offerDocument.DocumentNode.SelectSingleNode("//div[@data-test='section-offerValidTo']")?.InnerText.Trim() ?? "Brak danych",
                Salary = offerDocument.DocumentNode.SelectSingleNode("//div[@data-test='section-contract']")?.InnerText.Trim() ?? "Brak danych",
                ComapnyImgUrl = offerDocument.DocumentNode.SelectSingleNode("//img[@data-test='icon-companyLogo']")?.GetAttributeValue("src", string.Empty) ?? "Brak danych",
                AboutCompany = offerDocument.DocumentNode.SelectSingleNode("//div[@data-test='section-about-us-description']")?.InnerText.Trim() ?? "Brak danych",
                Url = url,
                Description = description
            };
        }

        protected override async Task<string> GetHtmlAsync(string url)
        {
            const int maxRetries = 5;
            const int baseDelay = 5000; 

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    await Task.Delay(new Random().Next(2000, 5000)); 
                    return await _client.GetStringAsync(url);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Błąd podczas pobierania strony (próba {attempt + 1}): {e.Message}");
                    if (attempt < maxRetries - 1)
                    {
                        await Task.Delay(baseDelay * (attempt + 1)); 
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return string.Empty;
        }

        private JobOfferDescription GetTheProtocolOfferDescription(HtmlDocument offerDocument)
        {
            var jobResponsibilities = ExtractList(offerDocument, "//div[@data-test='section-responsibilities']");
            var jobRequirements = ExtractList(offerDocument, "//div[@data-test='section-requirements']");
            var jobOffered = ExtractList(offerDocument, "//div[@data-test='section-offered']");
            var jobBenefits = ExtractList(offerDocument, "//div[@data-test='section-benefits']");

            var technologiesSection = offerDocument.DocumentNode.SelectSingleNode("//div[@data-test='section-technologies']");
            var jobTechnologies = new List<string>();
            if (technologiesSection != null)
            {
                var technologies = technologiesSection.SelectNodes(".//div[@data-test='chip-technology']");
                if (technologies != null)
                {
                    foreach (var technology in technologies)
                    {
                        jobTechnologies.Add(technology.InnerText.Trim());
                    }
                }
            }

            return new JobOfferDescription
            {
                Responsibilities = jobResponsibilities,
                Requirements = jobRequirements,
                Technologies = jobTechnologies.Count > 0 ? jobTechnologies : new List<string> { "Brak danych" },
                Offered = jobOffered,
                Benefits = jobBenefits
            };
        }

        private List<string> ExtractList(HtmlDocument document, string xpath)
        {
            var list = new List<string>();
            var section = document.DocumentNode.SelectSingleNode(xpath);
            if (section != null)
            {
                var items = section.SelectNodes(".//li[@data-test='text-sectionItem']");
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        list.Add(item.InnerText.Trim());
                    }
                }
            }
            return list.Count > 0 ? list : new List<string> { "Brak danych" };
        }
    }
}
