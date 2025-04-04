//using System.Drawing;
//using DevExpress.XtraReports.UI;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.Extensions.Configuration; // ? Ensure this is included
//using QD.ERP.Web.Areas.Finance.Reports.AccountRegister;
//using QD.ERP.Web.Areas.Finance.Reports.cashPayments;
//using QD.ERP.Web.Areas.Finance.Reports.Cost_Analysis;
//using QD.ERP.Web.Areas.Finance.Reports.test;
//using QD.ERP.Web.DAL.Entities;

//namespace QD.ERP.Web.Pages
//{
//    public class CostAllocationModel : PageModel
//    {
//        private readonly DAL.Entities.ERPMasterWtDataContext _eRPMasterWtDataContext;
//        private readonly IConfiguration _configuration; // ? Inject configuration

//        public Tbl901CompanyDetail ERPCompany_details;
//        public XtraReport Report { get; set; }
//        public DateTime FrmDate { get; private set; }
//        public DateTime ToDate { get; private set; }
//        public List<string> SelectedValues { get; private set; } = new List<string>();
//        public string CostAllocationMasterGroup { get; set; }

//        // ? Constructor Injection
//        public CostAllocationModel(DAL.Entities.ERPMasterWtDataContext eRPMasterWtDataContext, IConfiguration configuration)
//        {
//            _eRPMasterWtDataContext = eRPMasterWtDataContext;
//            _configuration = configuration; // ? Store injected configuration
//        }

//        public IActionResult OnGet(string reportName, string costAllocationMasterGroup, DateTime? frmDate, DateTime? toDate, string[] selectedValues)
//        {
//            if (string.IsNullOrEmpty(reportName) || string.IsNullOrEmpty(costAllocationMasterGroup))
//            {
//                return BadRequest("Invalid report name or costAllocationMasterGroup");
//            }

//            CostAllocationMasterGroup = costAllocationMasterGroup;

//            var tenantName = HttpContext.Session.GetString("TenentName") ?? "Default Tenant";
//            var ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails.FirstOrDefault(x => x.CompanyNameShort == tenantName);

//            // **Set default values if company details are not found**
//            var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
//            var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
//            var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
//            var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;
//            Image logoImage = null;

//            if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
//            {
//                try
//                {
//                    using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
//                    {
//                        logoImage = Image.FromStream(ms);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine("Error processing company logo: " + ex.Message);
//                }
//            }

//            if (!string.IsNullOrEmpty(costAllocationMasterGroup) && frmDate.HasValue && toDate.HasValue)
//            {
//                CostAllocationMasterGroup = costAllocationMasterGroup;
//                FrmDate = frmDate.Value;
//                ToDate = toDate.Value;

//                switch (reportName)
//                {
//                    case "CostCenterSummaryReport":
//                        Report = new CostCenterSummaryReport(
//                            _configuration,  // ? Use injected configuration
//                            CostAllocationMasterGroup,
//                            FrmDate,
//                            ToDate,
//                            tenantName,
//                            companyName,
//                            companyAddress,
//                            logoImage,
//                            companyNameAr,
//                            companyAddressAr
//                        );
//                        break;

//                    default:
//                        return NotFound("Report not found.");
//                }
//            }

//            return Page();
//        }
//    }
//}
