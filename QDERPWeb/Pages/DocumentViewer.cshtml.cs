using DevExpress.CodeParser.VB;
using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using QD.ERP.Web.Areas.Finance.Reports;
using QD.ERP.Web.Areas.Finance.Reports.Payable_Statements;
using QD.ERP.Web.Areas.Finance.Reports.Receivable_Statements;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Reports;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing;
using QD.ERP.Web.Areas.Finance.Reports.BillsReceivable;
using QD.ERP.Web.Areas.Finance.Reports.TrialBalance;
using QD.ERP.Web.Areas.Finance.Reports.Register;
using QD.ERP.Web.Areas.Finance.Reports.AccountStatement;
using QD.ERP.Web.Areas.Finance.Reports.TrialBalance.AgeingReport;
using QD.ERP.Web.Areas.Finance.Reports.TrialBalance.AgeingReports;

namespace QD.ERP.Web.Pages
{
	public class DocumentViewerModel : PageModel
	{
		public XtraReport Report { get; private set; }
		public string ReportName { get; private set; }
		public string AccountId { get; private set; }
        public string AccountGroup { get; private set; }
        public DateTime FrmDate { get; private set; }
		public DateTime ToDate { get; private set; }

		private readonly TenantDbContextHelper _tenantDbContextHelper;
		private ERPMasterWtDataContext _eRPMasterWtDataContext;
		private Tbl901CompanyDetail ERPCompany_details;



		public DocumentViewerModel(TenantDbContextHelper tenantDbContextHelper)
		{
			_tenantDbContextHelper = tenantDbContextHelper;
		}

		public IActionResult OnGet(string reportName, string accountId, DateTime? frmDate, DateTime? toDate,string accountGroup)
		
        {
			if (string.IsNullOrEmpty(reportName))
			{
				return BadRequest("Invalid report name.");
			}

			// Try to get tenant and DB context once
			if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
			{
				return StatusCode(500, "Tenant not found or DbContext could not be created.");
			}

			_eRPMasterWtDataContext = dbContext;
			ReportName = reportName;

			if (reportName == "StatementOfAccountReport")
			{
				if (string.IsNullOrEmpty(accountId) || frmDate == null || toDate == null)
				{
					return BadRequest("Missing required parameters for StatementOfAccountReport.");
				}

				AccountId = accountId;
				FrmDate = frmDate.Value;
				ToDate = toDate.Value;

				var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

                var companyDetails = _eRPMasterWtDataContext.Tbl901CompanyDetails
					.FirstOrDefault(x => x.CompanyNameShort == tenantName);

				string companyName = companyDetails?.CompanyName ?? string.Empty;
				string companyAddress = companyDetails?.CompanyFullAddress ?? string.Empty;
				string companyAddressAr = companyDetails?.CompanyFullAddressAr ?? string.Empty;
				string companyNameAr = companyDetails?.CompanyNameAr ?? string.Empty;

				Image logoImage = null;
				if (companyDetails?.CompanyLogo is byte[] logoBytes && logoBytes.Length > 0)
				{
					try
					{
						using (var ms = new MemoryStream(logoBytes))
						{
							logoImage = Image.FromStream(ms);
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("Error processing company logo: " + ex.Message);
					}
				}

				Report = new StatementOfAccountReport(
					AccountId,
					FrmDate,
					ToDate,
					tenantName,
					companyName,
					companyAddress,
					logoImage,
					companyNameAr,
					companyAddressAr,
                    userName,
                    _tenantDbContextHelper
				);
			}

		


            else if (reportName == "AccountWithNarration")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for AccountWithNarration.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

               
                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms); 
                        }
                    }
                    catch (Exception ex)
                    {
                       
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new AccountWithNarration(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr,  userName, _tenantDbContextHelper
                );
            }
            else if (reportName == "AccountStatementFormat2Report")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for AccountStatementFormat2Report.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null; 

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms); 
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new AccountStatementFormat2Report(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr,userName, _tenantDbContextHelper
				);
            }
            else if (reportName == "AccountExportFromatReport")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for AccountExportFromatReport.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
           .FirstOrDefault(x => x.CompanyNameShort == tenantName);

       var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
       var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
       var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
       var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

       string logoBase64 = string.Empty;
       Image logoImage = null;

       if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
       {
           try
           {
               using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
               {
                   logoImage = Image.FromStream(ms);
               }
           }
           catch (Exception ex)
           {
               Console.WriteLine("Error processing company logo: " + ex.Message);
           }
       }
       Report = new AccountExportFromatReport(
           AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
           companyNameAr, companyAddressAr,userName, _tenantDbContextHelper
	   );
               
            }
            else if (reportName == "AccountOrderbyVchNoWONarrationReport")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for AccountOrderbyVchNoWONarrationReport.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new AccountOrderbyVchNoWONarrationReport(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr,userName, _tenantDbContextHelper
				);


            }
            else if (reportName == "AccountExportLandscapeReport")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for AccountExportLandscapeReport.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new AccountExportLandscapeReport(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr,userName, _tenantDbContextHelper
				);


            }
            else if (reportName == "AccountOrderbyVchNoWONarrationReport")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for AccountOrderbyVchNoWONarrationReport.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                //Report = new AccountOrderbyVchNoWONarrationReport(AccountId, FrmDate, ToDate);
            }
            else if (reportName == "AccountDebtorsReport")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for AccountOrderByVoucherNo.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                var auditorName = ERPCompany_details?.AuditorName ?? string.Empty;
                var auditorAddress = ERPCompany_details?.AuditorAddress ?? string.Empty;
                var auditorEmail = ERPCompany_details?.AuditorEmail ?? string.Empty;

                Image logoImage = null;
                Image companySealImage = null;

                try
                {
                    if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }

                    if (ERPCompany_details?.CompanySeal != null && ERPCompany_details.CompanySeal.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanySeal))
                        {
                            companySealImage = Image.FromStream(ms);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing company images: " + ex.Message);
                }

                Report = new AccountDebtorsReport(
                    AccountId, FrmDate, ToDate, tenantName,
                    companyName, companyAddress, logoImage, companySealImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper,
                    auditorName, auditorAddress, auditorEmail
                );
            }
            else if (reportName == "AccountOrderByVoucherNo")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for AccountOrderByVoucherNo.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new AccountOrderByVoucherNo(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr,userName, _tenantDbContextHelper
				);

            }
            else if (reportName == "AccountDetails")

            {


                if (accountId == null || frmDate == null || toDate == null)

                {

                    return BadRequest("Missing required parameters for AccountDetails.");

                }


                AccountId = accountId;

                FrmDate = frmDate.Value;

                ToDate = toDate.Value;

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new AccountDetails(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr,userName, _tenantDbContextHelper
				);

            }


            else if (reportName == "AccountDebtors")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for AccountOrderByVoucherNo.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                var auditorName = ERPCompany_details?.AuditorName ?? string.Empty;
                var auditorAddress = ERPCompany_details?.AuditorAddress ?? string.Empty;
                var auditorEmail = ERPCompany_details?.AuditorEmail ?? string.Empty;
                var auditorFaxNo = ERPCompany_details?.AuditorFaxNo ?? string.Empty;

                Image logoImage = null;
                Image companySealImage = null;

                try
                {
                    if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }

                    if (ERPCompany_details?.CompanySeal != null && ERPCompany_details.CompanySeal.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanySeal))
                        {
                            companySealImage = Image.FromStream(ms);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing company images: " + ex.Message);
                }

                Report = new AccountDebtors(
                    AccountId, FrmDate, ToDate, tenantName,
                    companyName, companyAddress, logoImage, companySealImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper,
                    auditorName, auditorAddress, auditorEmail, auditorFaxNo
                );
            }




            ///(Receivable Statements)

            else if (reportName == "BillsReceivablelandscapeformat")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for BillsReceivablelandscapeformat.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivablelandscapeformat(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, userName, _tenantDbContextHelper
                );
            }
            else if (reportName == "BillsReceivableAll")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for BillsReceivableAll.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivableAll(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, userName, _tenantDbContextHelper
                );
            }
            else if (reportName == "BillsReceivableByAccount")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for BillsReceivableAll.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivableByAccount(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, userName, _tenantDbContextHelper
                );
            }
            else if (reportName == "BillsReceivableAgeingToday")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for BillsReceivableAll.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";
                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }

                Report = new BillsReceivableAgeingToday(
                 AccountId, FrmDate, ToDate,tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, userName, _tenantDbContextHelper
                 );
             }


            else if (reportName == "BillsReceivableRentation")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for BillsReceivableAll.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivableRentation(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, userName, _tenantDbContextHelper
                );
            }

            else if (reportName == "BillsReceivableLedgerBalance")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for BillsReceivableAll.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivableLedgerBalance(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, userName, _tenantDbContextHelper
                );
            }

            else if (reportName == "BillsReceivableFormat")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for BillsReceivableAll.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";
                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
      

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivableFormat(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, userName, _tenantDbContextHelper
                );


            }
            else if (reportName == "Report4")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for BillsReceivableAll.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new Report4(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, userName, _tenantDbContextHelper
                );


            }


/////////////(Payable Statements)

            else if (reportName == "rpt201BillsPayable")

            {


                if (accountId == null || frmDate == null || toDate == null)

                {

                    return BadRequest("Missing required parameters for rpt201BillsPayable.");

                }


                AccountId = accountId;

                FrmDate = frmDate.Value;

                ToDate = toDate.Value;

                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";
                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;
            

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new rpt201BillsPayable(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, userName, _tenantDbContextHelper
                );

            }
            else if (reportName == "AgeingToday")

            {


                if (accountId == null || frmDate == null || toDate == null)

                {

                    return BadRequest("Missing required parameters for AgeingToday.");

                }


                AccountId = accountId;

                FrmDate = frmDate.Value;

                ToDate = toDate.Value;
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";
                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }

                Report = new AgeingToday(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr,userName,_tenantDbContextHelper
				);


            }

            else if (reportName == "rpt201BillsPayableWithVchNo")

            {


                if (accountId == null || frmDate == null || toDate == null)

                {

                    return BadRequest("Missing required parameters for rpt201BillsPayableWithVchNo.");

                }


                AccountId = accountId;

                FrmDate = frmDate.Value;

                ToDate = toDate.Value;


                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";
                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new rpt201BillsPayableWithVchNo(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr,userName ,_tenantDbContextHelper
                );

            }
            
            else if (reportName == "EndDate")

            {


                if (accountId == null || frmDate == null || toDate == null)

                {

                    return BadRequest("Missing required parameters for EndDate.");

                }


                AccountId = accountId;

                FrmDate = frmDate.Value;

                ToDate = toDate.Value;


                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";
                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }

                Report = new EndDate(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, userName, _tenantDbContextHelper
                );

            }
            else if (reportName == "Payablelandscape")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Payablelandscape.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }

                Report = new Payablelandscape(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper
                );
            }
            else if (reportName == "payableRetention")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Payablelandscape.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";
                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }

                Report = new payableRetention(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr,userName, _tenantDbContextHelper
                );
            }
            else if (reportName == "Balance")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Balance.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";
                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new Balance(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr,userName, _tenantDbContextHelper
                );
            }
            else if (reportName == "BillsPayablePaid")
            {
                if (accountId == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for BillsPayablePaid.");
                }

                AccountId = accountId;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;

                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";
                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }




                Report = new BillsPayablePaid(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr,userName, _tenantDbContextHelper
                );
            }
           
            
            
            ///Trial
            
            else if (reportName == "Group")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Group.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new Group(
                    AccountGroup, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper
                );
            }


            else if (reportName == "subGroup")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Group.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new subGroup(
                    AccountGroup, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper
                );
            }

            else if (reportName == "TrialBalanceReport")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Group.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new TrialBalanceReport(
                    AccountGroup, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper
                );
            }
            else if (reportName == "TrialBalanceExportFormat")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Group.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new TrialBalance_ExportFormat_(
                    AccountGroup, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper
                );
            }
            else if (reportName == "TrialBalanceDrCr")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Group.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new TrialBalanceDrCr(
                    AccountGroup, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper
                );
            }
            else if (reportName == "IncomeStatements")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for incomestatement.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new IncomeStatements(
                    AccountGroup, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper
                );
            }
            else if (reportName == "incomeStatementsBymonth")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Group.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new incomeStatements_Bymonth_(
                    AccountGroup, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper
                );
            }
            else if (reportName == "IncomestatementsHorizondal")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Group.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new IncomestatementsHorizondal(
                    AccountGroup, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper
                );
            }
            else if (reportName == "balnceSheet")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Group.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                var userName = HttpContext.Session.GetString("UserName") ?? "Default User";

                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new balnceSheet(
                    AccountGroup, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper,userName
                );

            }
            else if (reportName == "BalanceSheetHorizondalFormat")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Group.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BalanceSheetHorizondalFormat(
                    AccountGroup, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper
                );
            }
            //Financial Ageing Reports
            else if (reportName == "BillsPayablesAgeingasperLedgerBalanceByEndDate")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Group.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsPayablesAgeingasperLedgerBalanceByEndDate(
                    AccountGroup, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper
                );
            }

            else if (reportName == "BillsPayablesAgeingByEndDate")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Group.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsPayablesAgeingByEndDate(
                    AccountGroup, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper
                );
            }
            else if (reportName == "BillsReceivableEndDate")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Group.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivableEndDate(
                    AccountGroup, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper
                );
            }
            else if (reportName == "BillsPayablesSummaryAgeingasperLedgerBalanceByEndDate")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Group.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsPayablesSummaryAgeingasperLedgerBalanceByEndDate(
                    AccountGroup, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper
                );
            }
            else if (reportName == "BillsReceivableledgerBalance")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Group.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivableledgerBalance(
                    AccountGroup, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper
                );
            }
            else if (reportName == "BillsReceivablesReportByEndDate")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Group.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivablesReportByEndDate(
                    AccountGroup, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper
                );
            }
            else if (reportName == "BillsReceivablesSummaryAgeingasperLedgerBalanceByEndDate")
            {
                if (accountGroup == null || frmDate == null || toDate == null)
                {
                    return BadRequest("Missing required parameters for Group.");
                }

                AccountGroup = accountGroup;
                FrmDate = frmDate.Value;
                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);


                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms);
                        }
                    }
                    catch (Exception ex)
                    {

                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivablesSummaryAgeingasperLedgerBalanceByEndDate(AccountGroup, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr, _tenantDbContextHelper);


            }
            else
            {
                switch (reportName)
                {
                    case "XtraReportBillsReceivableAgeingReport":
                        Report = new XtraReportBillsReceivableAgeingReport();
                        break;
                    case "XtraReportAgeingreportsummary":
                        Report = new XtraReportAgeingreportsummary();
                        break;

                    default:
                        return NotFound("Report not found.");
                }
            }
            return Page();
        }
    }
}