using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace QD.ERP.Web.Service
{
    public class CurrencyRateDto
    {
        public string BaseCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public decimal ExchangeRate { get; set; }
    }

    public class CurrencyRateService
    {
        private readonly HttpClient _httpClient;

        public CurrencyRateService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://open.er-api.com/v6/")
            };
        }

        public async Task<List<CurrencyRateDto>> GetExchangeRatesAsync(string baseCurrency)
        {
            var response = await _httpClient.GetAsync($"latest/{baseCurrency}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);

            if (json.RootElement.GetProperty("result").GetString() != "success")
                return new List<CurrencyRateDto>(); // or throw exception

            var rates = json.RootElement.GetProperty("rates");

            var list = rates.EnumerateObject().Select(rate => new CurrencyRateDto
            {
                BaseCurrency = baseCurrency,
                TargetCurrency = rate.Name,
                ExchangeRate = rate.Value.GetDecimal()
            }).ToList();

            return list;
        }
    }



}
