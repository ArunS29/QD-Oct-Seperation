using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Areas.Finance.Reports.AccountRegister;
using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
using QD.ERP.Web.Areas.Finance.Reports.Receivable_Statements;
using QD.ERP.Web.Reports;

namespace QD.ERP.Web.Pages
{
    public class RegisterViewerModel : PageModel
    {
        public XtraReport Report { get; private set; }

        public string ReportName { get; private set; }

        public string VoucherType { get; private set; }
        public DateTime FrmDate { get; private set; }
        public DateTime ToDate { get; private set; }

        public IActionResult OnGet(string reportName, string voucherType, DateTime? frmDate, DateTime? toDate)
        {
            if (string.IsNullOrEmpty(reportName))
            {
                return BadRequest("Invalid report name.");
            }

            ReportName = reportName;


            if (reportName == "PreviewRegister")
            {
                if (string.IsNullOrEmpty(voucherType) || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for PreviewRegister.");
                }

                VoucherType = voucherType;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var companyName = HttpContext.Session.GetString("CompanyName") ?? "Default Company";
                var LogoUrl = HttpContext.Session.GetString("LogoUrl") ?? string.Empty;

                Report = new PreviewRegister(voucherType, FrmDate, ToDate);
            }
            else if (reportName == "OrderByVchNoRegister")
            {
                if (string.IsNullOrEmpty(voucherType) || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for OrderByVchNoRegister.");
                }

                VoucherType = voucherType;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var companyName = HttpContext.Session.GetString("CompanyName") ?? "Default Company";
                var LogoUrl = HttpContext.Session.GetString("LogoUrl") ?? string.Empty;

                Report = new OrderByVchNoRegister(voucherType, FrmDate, ToDate);
            }
            else if (reportName == "OrderbyVchNoWIthVchNarration")
            {
                if (string.IsNullOrEmpty(voucherType) || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for OrderbyVchNoWIthVchNarration.");
                }

                VoucherType = voucherType;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var companyName = HttpContext.Session.GetString("CompanyName") ?? "Default Company";
                var LogoUrl = HttpContext.Session.GetString("LogoUrl") ?? string.Empty;

                Report = new OrderbyVchNoWIthVchNarration(voucherType, FrmDate, ToDate);
            }
            else if (reportName == "Register4line")
            {
                if (string.IsNullOrEmpty(voucherType) || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Register4line.");
                }

                VoucherType = voucherType;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var companyName = HttpContext.Session.GetString("CompanyName") ?? "Default Company";
                var LogoUrl = HttpContext.Session.GetString("LogoUrl") ?? string.Empty;

                Report = new Register4line(voucherType, FrmDate, ToDate);
            }
            else if (reportName == "RegisterLineEntryNarration")
            {
                if (string.IsNullOrEmpty(voucherType) || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for RegisterLineEntryNarration.");
                }

                VoucherType = voucherType;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var companyName = HttpContext.Session.GetString("CompanyName") ?? "Default Company";
                var LogoUrl = HttpContext.Session.GetString("LogoUrl") ?? string.Empty;

                Report = new RegisterLineEntryNarration(voucherType, FrmDate, ToDate);
            }
            else if (reportName == "RegisterWithVchNarration")
            {
                if (string.IsNullOrEmpty(voucherType) || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for RegisterWithVchNarration.");
                }

                VoucherType = voucherType;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var companyName = HttpContext.Session.GetString("CompanyName") ?? "Default Company";
                var LogoUrl = HttpContext.Session.GetString("LogoUrl") ?? string.Empty;

                Report = new RegisterWithVchNarration(voucherType, FrmDate, ToDate);
            }
            else
            {
                switch (reportName)
                {
                    case "ReceivableReport(EffectiveDate)":
                        Report = new ReceivableReport_EffectiveDate_();
                        break;
                    case "XtraRecivableReport":
                        Report = new XtraRecivableReport();
                        break;

                    default:
                        return NotFound("Report not found.");
                }
            }
            return Page();
        }
    }
}