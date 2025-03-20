using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.Areas.Finance.Reports.cashPayments;
using QD.ERP.Web.Areas.Finance.Reports.test;

namespace QD.ERP.Web.Pages
{
    public class VoucherViewerModel : PageModel
    {
        public XtraReport Report { get; private set; }
        public string VoucherNo { get; private set; }

        public IActionResult OnGet(string reportName, string voucherNo)
        {
            if (string.IsNullOrEmpty(reportName) || string.IsNullOrEmpty(voucherNo))
            {
                return BadRequest("Invalid report name or voucher number.");
            }

            VoucherNo = voucherNo;

            switch (reportName)
            {
                case "cashPaymentformat2":
                    Report = new cashPaymentformat2(VoucherNo);  // Pass voucher number
                    break;
                case "cashPayments":
                    Report = new cashPayments(VoucherNo);  // Pass voucher number
                    break;
                default:
                    return NotFound("Report not found.");
            }

            return Page();
        }
    }
}
