using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.Finance.Pages
{
    public class VoucherBankPaymentModel : PageModel
    {
        private readonly CurrencyService _currencyService;

        public List<CurrencyMaster> Currencies { get; set; }

        public VoucherBankPaymentModel(CurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        public void OnGet()
        {
            // Retrieve the list of currencies using the CurrencyService
            Currencies = _currencyService.GetCurrencies();
        }
    }
}
