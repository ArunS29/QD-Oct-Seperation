using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Areas.Finance.Reports.BillsReceivable;
using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
using QD.ERP.Web.Areas.Finance.Reports.Receivable_Statements;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Reports;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace QD.ERP.Web.Pages
{
	public class ReportDesignerModel : PageModel
	{
		private readonly TenantDbContextHelper _tenantDbContextHelper;
		private ERPMasterWtDataContext _eRPMasterWtDataContext;
		private Tbl901CompanyDetail ERPCompany_details;

		public XtraReport Report { get; private set; }
		public string ReportName { get; private set; }

		public ReportDesignerModel(TenantDbContextHelper tenantDbContextHelper)
		{
			_tenantDbContextHelper = tenantDbContextHelper;
		}

		public IActionResult OnGet(string reportName, string accountId, DateTime? frmDate, DateTime? toDate)
		{
			if (string.IsNullOrEmpty(reportName))
				return BadRequest("Invalid report name.");

			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
			{
				return StatusCode(500, "Tenant not found or DbContext could not be created.");
			}

			_eRPMasterWtDataContext = dbContext;
			ReportName = reportName;

			string tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
			ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
				.FirstOrDefault(x => x.CompanyNameShort == tenantName);

			string companyName = ERPCompany_details?.CompanyName ?? string.Empty;
			string companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
			string companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
			string companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

			Image logoImage = null;
			if (ERPCompany_details?.CompanyLogo is byte[] logoBytes && logoBytes.Length > 0)
			{
				try
				{
					using var ms = new MemoryStream(logoBytes);
					logoImage = Image.FromStream(ms);
				}
				catch (Exception ex)
				{
					Console.WriteLine("Error loading company logo: " + ex.Message);
				}
			}

			var parameterizedReports = new Dictionary<string, Func<string, DateTime, DateTime, XtraReport>>()
			{
				{ "StatementOfAccountReport", (id, from, to) => new StatementOfAccountReport(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				{ "AccountWithNarration", (id, from, to) => new AccountWithNarration(id, from, to, "", "", "", null, "", "",_tenantDbContextHelper) },
				{ "AccountDetails", (id, from, to) => new AccountDetails(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				{ "AccountOrderByVoucherNo",( id, from, to)=> new AccountOrderByVoucherNo(id, from, to,"", "", "", null, "", "", _tenantDbContextHelper) },
				{ "AccountExportFromatReport", (id, from, to) => new AccountExportFromatReport(id, from, to,"", "", "", null, "", "",_tenantDbContextHelper) },
				{ "AccountExportLandscapeReport", (id, from, to) => new AccountExportLandscapeReport(id, from, to,"", "", "", null, "", "", _tenantDbContextHelper) },
				{ "AccountStatementFormat2Report", (id, from, to) => new AccountStatementFormat2Report(id, from, to, "", "", "", null, "", "",_tenantDbContextHelper) },
				{ "AccountOrderbyVchNoWONarrationReport", (id, from, to) => new AccountOrderbyVchNoWONarrationReport(id, from, to,"", "", "", null, "", "", _tenantDbContextHelper) },
				{ "BillsReceivablelandscapeformat", (id, from, to) => new BillsReceivablelandscapeformat(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				{ "BillsReceivableLedgerBalance", (id, from, to) => new BillsReceivableLedgerBalance(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				{ "BillsReceivableRentation", (id, from, to) => new BillsReceivableRentation(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				{ "BillsReceivableAgeingToday", (id, from, to) => new BillsReceivableAgeingToday(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				{ "BillsReceivableByAccount", (id, from, to) => new BillsReceivableByAccount(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				{ "BillsReceivableAll", (id, from, to) => new BillsReceivableAll(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				{ "BillsReceivableFormat", (id, from, to) => new BillsReceivableFormat(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				{ "Report4", (id, from, to) => new Report4(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				{ "rpt201BillsPayable", (id, from, to) => new rpt201BillsPayable(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				{ "rpt201BillsPayableWithVchNo", (id, from, to) => new rpt201BillsPayableWithVchNo(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				//{ "AgeingToday", (id, from, to) => new AgeingToday(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				{ "EndDate", (id, from, to) => new EndDate(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				{ "Payablelandscape",( id, from, to)=> new Payablelandscape(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				{ "payableRetention",(id,from,to )=>new payableRetention(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				{ "Balance",(id,from,to )=>new Balance(id, from, to,"","","",null,"","",_tenantDbContextHelper) },
				{ "BillsPayablePaid",(id,from,to )=>new BillsPayablePaid(id, from, to,"","","",null,"","",_tenantDbContextHelper) }
			};

			var simpleReports = new Dictionary<string, Func<XtraReport>>()
			{
				{ "XtraReportBillsReceivableAgeingReport", () => new XtraReportBillsReceivableAgeingReport() },
				{ "XtraReportAgeingreportsummary", () => new XtraReportAgeingreportsummary() }
			};

			var reportsRequiringParameters = new HashSet<string>()
			{
				"StatementOfAccountReport", "AccountWithNarration", "AccountExportFromatReport",
				"AccountExportLandscapeReport", "AccountStatementFormat2Report",
				"AccountOrderbyVchNoWONarrationReport", "BillsReceivablelandscapeformat",
				"BillsReceivableLedgerBalance", "BillsReceivableRentation",
				"BillsReceivableAgeingToday", "BillsReceivableByAccount",
				"BillsReceivableAll", "BillsReceivableFormat", "rpt201BillsPayable",
				"rpt201BillsPayableWithVchNo", "AgeingToday", "EndDate", "Report4",
				"AccountDetails","AccountOrderByVoucherNo","Payablelandscape","payableRetention","Balance",
				"BillsPayablePaid","AgeingReport","BIllsPayable","ReceivableReport_EffectiveDate_","XtraRecivableReport",
				"XtraReportAgeingreportsummary","XtraReportBillsReceivableAgeingReport"
			};

			if (reportsRequiringParameters.Contains(reportName))
			{
				if (string.IsNullOrEmpty(accountId) || frmDate == null || toDate == null)
					return BadRequest($"Missing required parameters for {reportName}.");

				Report = parameterizedReports.ContainsKey(reportName)
					? parameterizedReports[reportName](accountId, frmDate.Value, toDate.Value)
					: null;
			}
			else
			{
				Report = simpleReports.ContainsKey(reportName)
					? simpleReports[reportName]()
					: null;
			}

			return Report == null ? NotFound("Report not found.") : Page();
		}
	}
}
