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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.General.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CompanyDetailsController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<CompanyDetailsController> _logger;

        public CompanyDetailsController(ILogger<CompanyDetailsController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ValidateCurrentPassword([FromBody] string currentPassword)
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

                // Compare plaintext password (or use hashed check if using hashing)
                if (user.Password != currentPassword)
                    return BadRequest("Current password is incorrect.");

                return Ok(); // Valid password
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating current password.");
                return StatusCode(500, "Internal server error.");
            }
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

                return Ok("Password changed  successfully.");
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

           // [Required]
            public string CompanyCity { get; set; }
            public string CompanyPhone { get; set; }

           // [Required]
            public string CompanyNameShort { get; set; }
            public string CompanyFax { get; set; }
            public string EmailAddress { get; set; }
            public string Website { get; set; }
            public string CurrencyAbbr { get; set; }
            public string CompanyFullAddress { get; set; }
            public string CompanyFullAddressAr { get; set; }
            public string ProductName { get; set; }
            public string CompanySlogan { get; set; }

            //[Required]
            public string CompanyVatno { get; set; }
            public string SellerGroupVatnumber { get; set; }
            public string CompanyTin { get; set; }

          //  [Required]
            public string SellerOtherIdtype { get; set; }

          //  [Required]
            public string SellerOtherSellerId { get; set; }
            public string SellerBuildingNumber { get; set; }
            public string SellerAdditionalNumber { get; set; }
            public string SellerProvince { get; set; }
            public string SellerProvinceAr { get; set; }
            public string SellerPostalCode { get; set; }

          //  [Required]
            public string SellerNeighborhood { get; set; }
            public string SellerNeighborhoodAr { get; set; }
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


        // 1. Get all company IDs (or short names as key, as you prefer)
        [HttpGet]
        public IActionResult GetAllCompanyIds()
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                return Unauthorized(new { message = "Invalid tenant context." });

            var companyIds = dbContext.Tbl901CompanyDetails
                .OrderBy(c => c.CompanyId) // or any other sort order
                .Select(c => c.CompanyId)  // or use CompanyNameShort if that's your key
                .ToList();

            return Ok(companyIds);
        }

        // 2. Get details by ID (now supports ?companyId=)
        [HttpGet]
        public async Task<IActionResult> GetAllCompanyDetails1([FromQuery] int? companyId, DataSourceLoadOptions loadOptions)
        {
            try
            {
                string tenantName = HttpContext.Session.GetString("TenantName");

                if (string.IsNullOrEmpty(tenantName))
                    return Unauthorized(new { message = "Tenant name not found in session.", success = false });

                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                    return Unauthorized(new { message = "Invalid tenant context.", success = false });

                var matchingCompanyQuery = dbContext.Tbl901CompanyDetails
                    .AsNoTracking();

                if (companyId.HasValue)
                    matchingCompanyQuery = matchingCompanyQuery.Where(c => c.CompanyId == companyId.Value);

                // ---- All your select as before (unchanged) ----
                var projection = matchingCompanyQuery.Select(c => new
                {
                    c.CompanyId,
                    c.CompanyAddress1Ar,
                    c.CompanyNameAr,
                    c.CompanyFullAddressAr,
                    c.CompanyAddress2Ar,
                    c.CompanyName,
                    c.CompanyCityAr,
                    c.SellerNeighborhoodAr,
                    c.CompanyAddress1,
                    c.CompanyAddress2,
                    c.CompanyCity,
                    c.CompanyPhone,
                    c.CompanyNameShort,
                    c.CompanyFax,
                    c.SellerCountryCode,
                    c.CompanyVatno,
                    c.SellerBuildingNumber,
                    c.SellerAdditionalNumber,
                    c.SellerProvince,
                    c.SellerProvinceAr,
                    c.SellerPostalCode,
                    c.SellerNeighborhood,
                    c.EmailAddress,
                    c.Website,
                    c.CurrencyAbbr,
                    c.CompanyFullAddress,
                    c.ProductName,
                    c.CompanySlogan,
                    c.CompanyShortNameAr,
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
                    LetterHead = c.LetterHead != null ? Convert.ToBase64String(c.LetterHead) : null,
                });

                var result = await DataSourceLoader.LoadAsync(projection, loadOptions);
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
                company.SellerProvinceAr = updatedCompany.SellerProvinceAr;
                company.SellerPostalCode = updatedCompany.SellerPostalCode;
                company.SellerNeighborhood = updatedCompany.SellerNeighborhood;
                company.SellerNeighborhoodAr = updatedCompany.SellerNeighborhoodAr;
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

        [HttpGet]
        public async Task<IActionResult> GetCountryCode()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var countrycode = await dbContext.Tbl00107CountryCodes
                        .Select(i => new
                        {
                            i.CountryCodeAlpha2,
                            i.CountryName,
                           


                        })
                        .ToListAsync();

                    return Ok(countrycode); // return raw data, paging/sorting done on client-side
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        } 

        [HttpGet]
        public async Task<IActionResult> GetIdType()
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var idType = await dbContext.Tbl00104SellerIdtypes
                        .Select(i => new
                        {
                            i.SellerIdtypeName,
                            i.SellerOtherIdtype,



                        })
                        .ToListAsync();

                    return Ok(idType); 
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while loading data.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserEntryLog(DateTime? fromDate, DateTime? toDate)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var query = dbContext.Tbl90116UserEntryLogSheets.AsQueryable();


                // Default dates if not provided
                if (!fromDate.HasValue)
                {
                    fromDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1); // Start of the current month
                }

                if (!toDate.HasValue)
                {
                    toDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)); // End of the current month
                }

                // Filtering by date range
                query = query.Where(i => i.LogCreatedOn >= fromDate && i.LogCreatedOn <= toDate);

                // Fetching the data
                var data = await query.Select(i => new
                {
                    i.UserEntryLogNo,
                    i.EntryLogFor,
                    i.EntryLogDetails,
                    i.LogCreatedOn,
                    i.LogCreatedBy,
                    i.LogDocumentNo,

                }).ToListAsync();


                return Ok(data);
            }

            return Unauthorized(new { message = "Invalid tenant." });
        }

        public IActionResult ChangePassword()
        {
            return PartialView("~/Areas/General/Pages/frm90108ChangePassword.cshtml"); // Use a partial view
        }




    }



}


