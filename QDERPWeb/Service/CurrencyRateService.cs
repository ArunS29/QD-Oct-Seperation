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

        public async Task<List<CurrencyRateDto>> GetInvertedExchangeRatesAsync(string baseCurrency, List<string> targetCurrencies)
        {
            var response = await _httpClient.GetAsync($"latest/{baseCurrency}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);

            if (json.RootElement.GetProperty("result").GetString() != "success")
                return new List<CurrencyRateDto>();

            var rates = json.RootElement.GetProperty("rates");

            // USD to INR = 83 → You want INR to USD → 1 / 83
            return targetCurrencies.Select(code =>
            {
                decimal rate = rates.GetProperty(code).GetDecimal(); // e.g., INR=83
                return new CurrencyRateDto
                {
                    BaseCurrency = baseCurrency,
                    TargetCurrency = code,
                    ExchangeRate = rate != 0 ? 1 / rate : 0
                };
            }).ToList();
        }

    }



}
