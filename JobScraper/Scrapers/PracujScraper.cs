using HtmlAgilityPack;
using JobScraper.Models.TheProtocol;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace JobScraper.Scrapers
{
    public class PracujScraper : BaseScraper, IJobScraper
    {
        private static readonly HttpClient _client = new HttpClient();
        private readonly string _url = "https://it.pracuj.pl/praca?et=1%2C3%2C17";
        private int _countOffers = 0;
        private int _pageNumber = 1;
        private readonly HashSet<string> _visitedUrls = new HashSet<string>();

        static PracujScraper()
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

                    var offersSection = document.DocumentNode.SelectSingleNode("//div[@data-test='section-offers']");
                    if (offersSection == null) break;

                    var offerNodes = offersSection.SelectNodes(".//div[@data-test='default-offer']");
                    if (offerNodes == null) break;

                    var offerTasks = new List<Task<JobOffer>>();

                    foreach (var offerNode in offerNodes)
                    {
                        var linkNode = offerNode.SelectSingleNode(".//a[@data-test='link-offer']");
                        var offerUrl = linkNode?.GetAttributeValue("href", string.Empty);
                        if (string.IsNullOrEmpty(offerUrl)) continue;

                        if (!_visitedUrls.Contains(offerUrl))
                        {
                            _visitedUrls.Add(offerUrl);
                            offerTasks.Add(ProcessOfferAsync(offerUrl));
                        }
                    }

                    var offers = await Task.WhenAll(offerTasks);
                    jobOffers.AddRange(offers);

                    await Task.Delay(15000);

                    // Paginacja
                    var nextPageNode = document.DocumentNode.SelectSingleNode("//button[@data-test='bottom-pagination-button-next']");
                    if (nextPageNode == null)
                        break;

                    _pageNumber++;
                    currentUrl = new Uri(_url + $"&pn={_pageNumber}").ToString();
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
            try
            {
                var offerHtml = await GetHtmlAsync(url);
                var offerDocument = LoadHtml(offerHtml);

                var titleNode = offerDocument.DocumentNode.SelectSingleNode("//h1[@data-test='text-positionName']");
                var companyNameNode = offerDocument.DocumentNode.SelectSingleNode("//h2[@data-test='text-employerName']");
                var workModesNode = offerDocument.DocumentNode.SelectSingleNode("//li[@data-scroll-id='work-modes']");
                var positionLevelsNode = offerDocument.DocumentNode.SelectSingleNode("//li[@data-scroll-id='position-levels']");
                var locationNode = offerDocument.DocumentNode.SelectSingleNode("//li[@data-scroll-id='workplaces']");
                var offerValidToNode = offerDocument.DocumentNode.SelectSingleNode("//li[@data-scroll-id='publication-dates']");
                var salaryNode = offerDocument.DocumentNode.SelectSingleNode("//div[@data-test='section-salary']");
                var companyImgUrlNode = offerDocument.DocumentNode.SelectSingleNode("//div[@data-test='section-company-logo']//img");
                var aboutCompanyNode = offerDocument.DocumentNode.SelectSingleNode("//section[@data-scroll-id='about-us-description-1']//div[contains(@class, 'c1s1xseq')]");

                var description = GetPracujOfferDescription(offerDocument);
                _countOffers++;
                Console.WriteLine($"Ilosc przetworznych ofert: {_countOffers}");

                return new JobOffer
                {
                    Title = titleNode?.InnerText.Trim() ?? "Brak danych",
                    CompanyName = companyNameNode?.InnerText.Trim() ?? "Brak danych",
                    WorkModes = workModesNode?.InnerText.Trim() ?? "Brak danych",
                    PositionLevels = positionLevelsNode?.InnerText.Trim() ?? "Brak danych",
                    Location = locationNode?.InnerText.Trim() ?? "Brak danych",
                    OfferValidTo = offerValidToNode?.InnerText.Trim() ?? "Brak danych",
                    Salary = salaryNode?.InnerText.Trim() ?? "Brak danych",
                    ComapnyImgUrl = companyImgUrlNode?.GetAttributeValue("src", string.Empty) ?? "Brak danych",
                    AboutCompany = aboutCompanyNode?.InnerText.Trim() ?? "Brak danych",
                    Url = url,
                    Description = description
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd podczas przetwarzania oferty {url}: {ex.Message}");
                return null;
            }
        }

        protected override async Task<string> GetHtmlAsync(string url)
        {
            const int maxRetries = 5;
            const int baseDelay = 8000;

            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    // Randomizowane opóźnienie przed zapytaniem
                    await Task.Delay(new Random().Next(4000, 10000));
                    return await _client.GetStringAsync(url);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine($"Błąd podczas pobierania strony (próba {attempt + 1}): {e.Message}");
                    Console.WriteLine(url);
                    if (attempt < maxRetries - 1)
                    {
                        // Opóźnienie przed kolejną próbą
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

        private JobOfferDescription GetPracujOfferDescription(HtmlDocument offerDocument)
        {
            var jobResponsibilities = ExtractList(offerDocument, "//section[@data-test='section-responsibilities']");
            var jobRequirements = ExtractList(offerDocument, "//section[@data-test='section-requirements']");
            var jobOffered = ExtractList(offerDocument, "//section[@data-test='section-offered']");

            var technologiesSection = offerDocument.DocumentNode.SelectSingleNode("//section[@data-test='section-technologies']");
            var jobTechnologies = new List<string>();
            if (technologiesSection != null)
            {
                var technologies = technologiesSection.SelectNodes(".//p[contains(@class, 'n1bzavn5')]");
                if (technologies != null)
                {
                    foreach (var technology in technologies)
                    {
                        jobTechnologies.Add(technology.InnerText.Trim());
                    }
                }
            }

            var benefitsSection = offerDocument.DocumentNode.SelectSingleNode("//section[@data-test='section-benefits']");
            var jobBenefits = new List<string>();
            if (benefitsSection != null)
            {
                var benefits = benefitsSection.SelectNodes(".//div[@data-test='text-benefit-title']");
                if (benefits != null)
                {
                    foreach (var benefit in benefits)
                    {
                        jobBenefits.Add(benefit.InnerText.Trim());
                    }
                }
            }

            return new JobOfferDescription
            {
                Responsibilities = jobResponsibilities,
                Requirements = jobRequirements,
                Technologies = jobTechnologies.Count > 0 ? jobTechnologies : new List<string> { "Brak danych" },
                Offered = jobOffered,
                Benefits = jobBenefits.Count > 0 ? jobBenefits : new List<string> { "Brak danych" }
            };
        }

        private List<string> ExtractList(HtmlDocument document, string xpath)
        {
            var list = new List<string>();
            var section = document.DocumentNode.SelectSingleNode(xpath);
            if (section != null)
            {
                var items = section.SelectNodes(".//li[contains(@class, 'tkzmjn3')]");
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
