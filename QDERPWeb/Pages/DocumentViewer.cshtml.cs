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

namespace QD.ERP.Web.Pages
{
    public class DocumentViewerModel : PageModel
    {
        public XtraReport Report { get; private set; }

        public string ReportName { get; private set; }

        public string AccountId { get; private set; }
        public DateTime FrmDate { get; private set; }
        public DateTime ToDate { get; private set; }

        private readonly DAL.Entities.ERPMasterWtDataContext _eRPMasterWtDataContext;


        public Tbl901CompanyDetail ERPCompany_details;
        public DocumentViewerModel(DAL.Entities.ERPMasterWtDataContext eRPMasterWtDataContext)
        {
            _eRPMasterWtDataContext = eRPMasterWtDataContext;

        }
        public IActionResult OnGet(string reportName, string accountId, DateTime? frmDate, DateTime? toDate)
        {
            if (string.IsNullOrEmpty(reportName))
            {
                return BadRequest("Invalid report name.");
            }

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
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                // **Set default values if company details are not found**
                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null; // Change from string to Image

                if (ERPCompany_details?.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms); // Convert Byte Array to Image
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error instead of returning a BadRequest (ensures the report still loads)
                        Console.WriteLine("Error processing company logo: " + ex.Message);
                    }
                }

                // **Pass values to the report even if company details are missing**
                Report = new StatementOfAccountReport(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                // **Set default values if company details are not found**
                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;  

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
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
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new AccountWithNarration(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                // **Set default values if company details are not found**
                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
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
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new AccountStatementFormat2Report(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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


                Report = new AccountExportFromatReport(AccountId, FrmDate, ToDate);
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


                Report = new AccountOrderbyVchNoWONarrationReport(AccountId, FrmDate, ToDate);
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


                Report = new AccountExportLandscapeReport(AccountId, FrmDate, ToDate);
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


                Report = new AccountOrderbyVchNoWONarrationReport(AccountId, FrmDate, ToDate);
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


                Report = new AccountOrderByVoucherNo(AccountId, FrmDate, ToDate);
            }

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
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                // **Set default values if company details are not found**
                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
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
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivablelandscapeformat(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                // **Set default values if company details are not found**
                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
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
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivablelandscapeformat(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                // **Set default values if company details are not found**
                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;
                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
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
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivableByAccount(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                // **Set default values if company details are not found**
                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
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
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivableAgeingToday(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                // **Set default values if company details are not found**
                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
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
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivableRentation(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                // **Set default values if company details are not found**
                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;
                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
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
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivableLedgerBalance(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                // **Set default values if company details are not found**
                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;
                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
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
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new BillsReceivableFormat(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
                );


            }
            else if (reportName == "rpt201BillsPayable")

            {


                if (accountId == null || frmDate == null || toDate == null)

                {

                    return BadRequest("Missing required parameters for rpt201BillsPayable.");

                }


                AccountId = accountId;

                FrmDate = frmDate.Value;

                ToDate = toDate.Value;


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                // **Set default values if company details are not found**
                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;
                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
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
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new rpt201BillsPayable(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);

                // **Set default values if company details are not found**
                var companyName = ERPCompany_details?.CompanyName ?? string.Empty;
                var companyAddress = ERPCompany_details?.CompanyFullAddress ?? string.Empty;
                var companyAddressAr = ERPCompany_details?.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details?.CompanyNameAr ?? string.Empty;
                string logoBase64 = string.Empty;
                Image logoImage = null;

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
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
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }
                Report = new rpt201BillsPayableWithVchNo(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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


                Report = new AccountDetails(AccountId, FrmDate, ToDate);

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

                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);
                if (ERPCompany_details == null)
                {
                    return BadRequest("Company details not found for the given tenant.");
                }

                var companyName = ERPCompany_details.CompanyName;
                var companyAddress = ERPCompany_details.CompanyFullAddress ?? "Default Company";
                var companyAddressAr = ERPCompany_details.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;  // Change from string to Image

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms); // Convert Byte Array to Image
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }



                // Pass the image object directly to the report
                Report = new StatementOfAccountReport(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);
                if (ERPCompany_details == null)
                {
                    return BadRequest("Company details not found for the given tenant.");
                }

                var companyName = ERPCompany_details.CompanyName;
                var companyAddress = ERPCompany_details.CompanyFullAddress ?? "Default Company";
                var companyAddressAr = ERPCompany_details.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;  // Change from string to Image

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms); // Convert Byte Array to Image
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }



                // Pass the image object directly to the report
                Report = new EndDate(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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
                if (ERPCompany_details == null)
                {
                    return BadRequest("Company details not found for the given tenant.");
                }

                var companyName = ERPCompany_details.CompanyName;
                var companyAddress = ERPCompany_details.CompanyFullAddress ?? "Default Company";
                var companyAddressAr = ERPCompany_details.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;  // Change from string to Image

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms); // Convert Byte Array to Image
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }



                // Pass the image object directly to the report
                Report = new Payablelandscape(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);
                if (ERPCompany_details == null)
                {
                    return BadRequest("Company details not found for the given tenant.");
                }

                var companyName = ERPCompany_details.CompanyName;
                var companyAddress = ERPCompany_details.CompanyFullAddress ?? "Default Company";
                var companyAddressAr = ERPCompany_details.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;  // Change from string to Image

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms); // Convert Byte Array to Image
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }



                // Pass the image object directly to the report
                Report = new payableRetention(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);
                if (ERPCompany_details == null)
                {
                    return BadRequest("Company details not found for the given tenant.");
                }

                var companyName = ERPCompany_details.CompanyName;
                var companyAddress = ERPCompany_details.CompanyFullAddress ?? "Default Company";
                var companyAddressAr = ERPCompany_details.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;  // Change from string to Image

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms); // Convert Byte Array to Image
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }



                // Pass the image object directly to the report
                Report = new Balance(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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


                var tenantName = HttpContext.Session.GetString("TenantName") ?? "Default Tenant";
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);
                if (ERPCompany_details == null)
                {
                    return BadRequest("Company details not found for the given tenant.");
                }

                var companyName = ERPCompany_details.CompanyName;
                var companyAddress = ERPCompany_details.CompanyFullAddress ?? "Default Company";
                var companyAddressAr = ERPCompany_details.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;  // Change from string to Image

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms); // Convert Byte Array to Image
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }



                // Pass the image object directly to the report
                Report = new BillsPayablePaid(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
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
                ERPCompany_details = _eRPMasterWtDataContext.Tbl901CompanyDetails
                    .FirstOrDefault(x => x.CompanyNameShort == tenantName);
                if (ERPCompany_details == null)
                {
                    return BadRequest("Company details not found for the given tenant.");
                }

                var companyName = ERPCompany_details.CompanyName;
                var companyAddress = ERPCompany_details.CompanyFullAddress ?? "Default Company";
                var companyAddressAr = ERPCompany_details.CompanyFullAddressAr ?? string.Empty;
                var companyNameAr = ERPCompany_details.CompanyNameAr ?? string.Empty;

                string logoBase64 = string.Empty;
                Image logoImage = null;  // Change from string to Image

                if (ERPCompany_details.CompanyLogo != null && ERPCompany_details.CompanyLogo.Length > 0)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream(ERPCompany_details.CompanyLogo))
                        {
                            logoImage = Image.FromStream(ms); // Convert Byte Array to Image
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest("Error processing company logo: " + ex.Message);
                    }
                }



                // Pass the image object directly to the report
                Report = new Report4(
                    AccountId, FrmDate, ToDate, tenantName, companyName, companyAddress, logoImage,
                    companyNameAr, companyAddressAr
                );


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