using DevExpress.XtraRichEdit.Import.Html;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using SkiaSharp;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.General.Controller
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CompanyDetailsController : ControllerBase
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<CompanyDetailsController> _logger;

        public CompanyDetailsController(ILogger<CompanyDetailsController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }


        [HttpPost]
        public async Task<IActionResult> ResetPassword( string newPassword)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized();

                string userIdStr = HttpContext.Session.GetString("UserId");
                if (!byte.TryParse(userIdStr, out byte currentUserId))
                    return Unauthorized("Invalid or missing UserId in session.");

                var user = await dbContext.TblUserMasters.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                if (user == null)
                    return NotFound("User not found.");

                // TODO: Hash the password before storing it!
                user.Password = newPassword;
                user.ModifiedOn = DateTime.UtcNow;
                user.ModifiedBy = user.UserName; // or get from session if you store the current username

                await dbContext.SaveChangesAsync();

                return Ok("Password reset successful.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password.");
                return StatusCode(500, "Internal server error.");
            }
        }



        [HttpGet]
        public IActionResult GetMaxCompanyID()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized();

            var maxCompanyId = dbContext.Tbl901CompanyDetails.Max(c => (int?)c.CompanyId) ?? 0;

            return Ok(new { maxCompanyId });
        }
        public class CompanyUpdateModel
        {
            public int CompanyId { get; set; }
            public string CompanyName { get; set; }
            public string CompanyAddress1 { get; set; }
            public string CompanyAddress2 { get; set; }
            public string CompanyCity { get; set; }
            public string CompanyPhone { get; set; }
            public string CompanyNameShort { get; set; }
            public string CompanyFax { get; set; }
            public string EmailAddress { get; set; }
            public string Website { get; set; }
            public string CurrencyAbbr { get; set; }
            public string CompanyFullAddress { get; set; }
            public string CompanyFullAddressAr { get; set; }
            public string ProductName { get; set; }
            public string CompanySlogan { get; set; }
            public string CompanyVatno { get; set; }
            public string SellerGroupVatnumber { get; set; }
            public string CompanyTin { get; set; }
            public string SellerOtherIdtype { get; set; }
            public string SellerOtherSellerId { get; set; }
            public string SellerBuildingNumber { get; set; }
            public string SellerAdditionalNumber { get; set; }
            public string SellerProvince { get; set; }
            public string SellerPostalCode { get; set; }
            public string SellerNeighborhood { get; set; }
            public string SellerCountryCode { get; set; }
            public string AuditorName { get; set; }
            public string AuditorFaxNo { get; set; }
            public string AuditorAddress { get; set; }
            public string AuditorEmail { get; set; }
            public string ErpdatabaseLocation { get; set; }
            public string Erpdmslocation { get; set; }
            public string ErpAutoBackupLocation { get; set; }
            public string BillsReceivableLocation { get; set; }
            public string BackupOperatorEmail { get; set; }
            public string OnlineBackupLocation { get; set; }
            public bool IsSendSuccessEmail { get; set; }
            public bool IsSendFailedEmail { get; set; }
            public bool IsOnlineBackup { get; set; }
            public bool IsBackupDatabaseOnly { get; set; }
            public string CompanyLogoBase64 { get; set; }
            public string CompanySealBase64 { get; set; }
            public string LetterHeadBase64 { get; set; }
            public string CompanyID_Ar { get; set; }
            // Arabic Fields (add these)
            public string CompanyNameAr { get; set; }
            public string CompanyAddress1Ar { get; set; }
            public string CompanyAddress2Ar { get; set; }
            public string CompanyCityAr { get; set; }
            public string CompanyShortNameAr { get; set; }

        }


        [HttpGet]
        public async Task<IActionResult> GetAllCompanyDetails1(DataSourceLoadOptions loadOptions)
        {
            try
            {
                // Get tenant name from session
                string tenantName = HttpContext.Session.GetString("TenantName");

                if (string.IsNullOrEmpty(tenantName))
                {
                    return Unauthorized(new { message = "Tenant name not found in session.", success = false });
                }

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    return Unauthorized(new { message = "Invalid tenant context.", success = false });
                }

                var matchingCompanyQuery = dbContext.Tbl901CompanyDetails
                    .AsNoTracking()
                    .Where(c => c.CompanyNameShort == tenantName)
                    .Select(c => new
                    {
                        c.CompanyId,
                        c.CompanyAddress1Ar,
                        c.CompanyNameAr,
                        c.CompanyFullAddressAr,
                        c.CompanyAddress2Ar,
                        c.CompanyName,
                        c.CompanyCityAr,
                        c.CompanyAddress1,
                        c.CompanyAddress2,
                        c.CompanyCity,
                        c.CompanyPhone,
                        c.CompanyNameShort,
                        c.CompanyFax,
                        c.CompanyVatno,
                        c.SellerBuildingNumber,
                        c.SellerAdditionalNumber,
                        c.SellerProvince,
                        c.SellerPostalCode,
                        c.SellerNeighborhood,
                        c.SellerCountryCode,
                        c.EmailAddress,
                        c.Website,
                        c.CurrencyAbbr,
                        c.CompanyFullAddress,
                        c.ProductName,
                        c.CompanySlogan,
                        c.SellerGroupVatnumber,
                        c.CompanyTin,
                        c.SellerOtherIdtype,
                        c.SellerOtherSellerId,
                        CompanyLogo = c.CompanyLogo != null ? Convert.ToBase64String(c.CompanyLogo) : null,
                        CompanySeal = c.CompanySeal != null ? Convert.ToBase64String(c.CompanySeal) : null,

                        c.AuditorName,
                        c.AuditorFaxNo,
                        c.AuditorAddress,
                        c.ErpdatabaseLocation,
                        c.Erpdmslocation,
                        c.ErpAutoBackupLocation,
                        c.BillsReceivableLocation,
                        c.BackupOperatorEmail,
                        c.OnlineBackupLocation,
                        c.IsSendSuccessEmail,
                        c.IsSendFailedEmail,
                        c.IsOnlineBackup,
                        c.IsBackupDatabaseOnly,
                        LetterHead=c.LetterHead !=null ? Convert.ToBase64String(c.LetterHead) :null,


                    });

                // ✅ Check existence *before* sending to DataSourceLoader
                if (!await matchingCompanyQuery.AnyAsync())
                {
                    return NotFound(new { success = false, message = $"Company with short name '{tenantName}' not found." });
                }

                // ✅ Pass IQueryable directly
                var result = await DataSourceLoader.LoadAsync(matchingCompanyQuery, loadOptions);
                return new JsonResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAllCompanyDetails1: {ex.Message}");
                return BadRequest(new { message = "An error occurred while fetching company details.", error = ex.Message });
            }
        }



        
        [HttpPost]
        public async Task<IActionResult> UpdateCompanyDetails([FromBody] CompanyUpdateModel updatedCompany)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized();

                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                    return BadRequest("Invalid model: " + errors);
                }

                var company = await dbContext.Tbl901CompanyDetails.FirstOrDefaultAsync(c => c.CompanyId == updatedCompany.CompanyId);
                if (company == null)
                    return NotFound("Company not found.");

                // Core Company Info
                company.CompanyName = updatedCompany.CompanyName;
                company.CompanyAddress1 = updatedCompany.CompanyAddress1;
                company.CompanyAddress2 = updatedCompany.CompanyAddress2;
                company.CompanyCity = updatedCompany.CompanyCity;
                company.CompanyPhone = updatedCompany.CompanyPhone;
                company.CompanyNameShort = updatedCompany.CompanyNameShort;
                company.CompanyFax = updatedCompany.CompanyFax;
                company.EmailAddress = updatedCompany.EmailAddress;
                company.Website = updatedCompany.Website;
                company.CurrencyAbbr = updatedCompany.CurrencyAbbr;

                // Additional Company Info
                company.CompanyFullAddress = updatedCompany.CompanyFullAddress;
                company.CompanyFullAddressAr = updatedCompany.CompanyFullAddressAr;
                company.ProductName = updatedCompany.ProductName;
                company.CompanySlogan = updatedCompany.CompanySlogan;

                // VAT & Identification
                company.CompanyVatno = updatedCompany.CompanyVatno;
                company.SellerGroupVatnumber = updatedCompany.SellerGroupVatnumber;
                company.CompanyTin = updatedCompany.CompanyTin;
                company.SellerOtherIdtype = updatedCompany.SellerOtherIdtype;
                company.SellerOtherSellerId = updatedCompany.SellerOtherSellerId;

                // Seller Addressing
                company.SellerBuildingNumber = updatedCompany.SellerBuildingNumber;
                company.SellerAdditionalNumber = updatedCompany.SellerAdditionalNumber;
                company.SellerProvince = updatedCompany.SellerProvince;
                company.SellerPostalCode = updatedCompany.SellerPostalCode;
                company.SellerNeighborhood = updatedCompany.SellerNeighborhood;
                company.SellerCountryCode = updatedCompany.SellerCountryCode;

                // Auditor Info
                company.AuditorName = updatedCompany.AuditorName;
                company.AuditorFaxNo = updatedCompany.AuditorFaxNo;
                company.AuditorAddress = updatedCompany.AuditorAddress;
                company.AuditorEmail = updatedCompany.AuditorEmail;

                // Backup Info
                company.ErpdatabaseLocation = updatedCompany.ErpdatabaseLocation;
                company.Erpdmslocation = updatedCompany.Erpdmslocation;
                company.ErpAutoBackupLocation = updatedCompany.ErpAutoBackupLocation;
                company.BillsReceivableLocation = updatedCompany.BillsReceivableLocation;
                company.BackupOperatorEmail = updatedCompany.BackupOperatorEmail;
                company.OnlineBackupLocation = updatedCompany.OnlineBackupLocation;
                company.IsSendSuccessEmail = updatedCompany.IsSendSuccessEmail;
                company.IsSendFailedEmail = updatedCompany.IsSendFailedEmail;
                company.IsOnlineBackup = updatedCompany.IsOnlineBackup;

                company.CompanyNameAr = updatedCompany.CompanyNameAr;
                company.CompanyAddress1Ar = updatedCompany.CompanyAddress1Ar;
                company.CompanyAddress2Ar = updatedCompany.CompanyAddress2Ar;
                company.CompanyCityAr = updatedCompany.CompanyCityAr;
                company.CompanyShortNameAr = updatedCompany.CompanyShortNameAr;

                // Company Logo
                if (!string.IsNullOrEmpty(updatedCompany.CompanyLogoBase64))
                    company.CompanyLogo = Convert.FromBase64String(updatedCompany.CompanyLogoBase64);

                if (!string.IsNullOrEmpty(updatedCompany.CompanySealBase64))
                    company.CompanySeal = Convert.FromBase64String(updatedCompany.CompanySealBase64);

                if (!string.IsNullOrEmpty(updatedCompany.LetterHeadBase64))
                    company.LetterHead = Convert.FromBase64String(updatedCompany.LetterHeadBase64);

                await dbContext.SaveChangesAsync();
                return Ok(new { message = "Company updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update company details.");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>


        [HttpGet]
        public async Task<IActionResult> GetComapnyDocuments()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var company = await dbContext.Tbl90102CompanyDocuments
                        .Select(i => new
                        {
                            i.CompDocumentType,
                            i.CompDocumentRefNo,
                            i.CompDocumentExpDate,
                            i.CompDocumentNotificationDate,
                            i.CompDocumentNo,
                           
                        })
                        .ToListAsync();

                    return Ok(company); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> VoucherDateLocking()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var datelock = await dbContext.Tbl90117VoucherDateLockings
                        .Select(i => new
                        {
                            i.VoucherTypeCode,
                            i.VoucherType,
                            i.VoucherModule,
                            i.VoucherDateLocked,
                            

                        })
                        .ToListAsync();

                    return Ok(datelock); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }
    }



}
