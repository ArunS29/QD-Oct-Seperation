using System;
using System.Linq;
using System.Threading.Tasks;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
using QD.ERP.Shared.Services.Logging;
using SkiaSharp;

namespace QD.ERP.Finance.Areas.Finance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountLedgerMasterController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<AccountLedgerMasterController> _logger;
        private readonly IUserActionLogger _userActionLogger;
        public AccountLedgerMasterController(ILogger<AccountLedgerMasterController> logger, IUserActionLogger userActionLogger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
            _userActionLogger = userActionLogger;
        }

        [HttpGet("getAccountingLedger")]
        public async Task<IActionResult> getAccountingLedger(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var qryListOfAccountlists = dbContext.Tbl201AccountGroups.Select(i => new
                    {
                        i.AccountGroupUnder,
                        i.AccountGroupId,
                        i.AccountGroup
                    });

                    var result = await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions);

                    if (loadOptions.Filter != null)
                    {
                        foreach (var filterItem in loadOptions.Filter)
                        {
                            var filterArray = filterItem as IList<object>;
                            if (filterArray != null && filterArray.Count >= 2)
                            {
                                if (filterArray[0].ToString() == "AccountGroupId")
                                {
                                    var accountGroupId = filterArray[1].ToString();
                                    qryListOfAccountlists = qryListOfAccountlists.Where(x => x.AccountGroupId == accountGroupId);
                                }
                            }
                        }
                    }

                    return Json(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in getAccountingLedger: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet("getCreditDebit")]
        public async Task<IActionResult> getCreditDebit(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var qryListOfOpeningBalanceDrCr = dbContext.Tbl201ChartOfAccounts
                        .Where(i => i.OpeningBalanceDrCr != null)
                        .Select(i => new { i.OpeningBalanceDrCr })
                        .Distinct();

                    return Json(await DataSourceLoader.LoadAsync(qryListOfOpeningBalanceDrCr, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in getCreditDebit: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet("getAccountBranch")]
        public async Task<IActionResult> getAccountBranch(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var accountbranchdetailsList = dbContext.Tbl20115CompanyBranches
                        .Select(i => new { i.BranchCode, i.BranchName });

                    return Json(await DataSourceLoader.LoadAsync(accountbranchdetailsList, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in getAccountBranch: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet("getSubGroup")]
        public async Task<IActionResult> getSubGroup(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var subgroupdetailsList = dbContext.Tbl20123LedgerSubGroups
                        .Select(i => new { i.LedgerSubGroupCode, i.SubGroupName });

                    return Json(await DataSourceLoader.LoadAsync(subgroupdetailsList, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in getSubGroup: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet("getSalesPersonList")]
        public async Task<IActionResult> getSalesPersonList(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var salesdetailsList = dbContext.Tbl20101SalesPersonMasters
                        .Select(i => new { i.SalesPersonCode, i.SalesPersonName });

                    return Json(await DataSourceLoader.LoadAsync(salesdetailsList, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in getSalesPersonList: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet("getCountryCode")]
        public async Task<IActionResult> getCountryCode(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var salesdetailsList = dbContext.Tbl00107CountryCodes
                        .Select(i => new { i.CountryCodeAlpha2, i.CountryName });

                    return Json(await DataSourceLoader.LoadAsync(salesdetailsList, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in getSalesPersonList: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }


        [HttpGet("getOtherIDTypes")]
        public async Task<IActionResult> getOtherIDTypes(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var buyerdetailsList = dbContext.Tbl00105BuyerIdtypes
                        .Select(i => new { i.BuyerOtherIdtype, i.BuyerIdtypeName });

                    return Json(await DataSourceLoader.LoadAsync(buyerdetailsList, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in getOtherIDTypes: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet("getUserName")]
        public IActionResult GetUserName()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var userName = HttpContext.Session.GetString("UserName") ?? "Unknown User"; // Get username from session
                    var userId = HttpContext.Session.GetString("UserId");
                    return Ok(new
                    {
                        success = true,
                        UserName = userName,
                        UserId = userId

                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in getUserName: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching user data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }



        [HttpGet("getBillingPayments")]
        public async Task<IActionResult> getBillingPayments(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var buyerdetailsList = dbContext.Qry20152BankAccounts
                        .Select(i => new { i.AccountId, i.AccountHead });

                    return Json(await DataSourceLoader.LoadAsync(buyerdetailsList, loadOptions));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in getBillingPayments: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet("GetLedgerAccounts")]
        public async Task<ActionResult> GetLedgerAccounts(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qryListOfAccountlists = dbContext.Qry201ListOfAccounts
                    .Where(p => p.AccountId != null)
                    .Select(i => new
                    {
                        i.MasterGroupId,
                        i.MasterGroup,
                        i.AccountGroup,
                        i.AccountGroupId,
                        i.AccountId,
                        i.AccountHead,
                        i.AccountHeadArabic,
                        i.ReferenceNo,
                        i.IsLedgerObselete
                    });

                return Json(await DataSourceLoader.LoadAsync(qryListOfAccountlists, loadOptions));
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet("GetMaxLedgerAccountID")]
        public async Task<ActionResult> GetMaxLedgerAccountID()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var nextAccountId = (from account in dbContext.Tbl201ChartOfAccounts
                                         where account.AccountId.StartsWith("L")
                                         orderby account.AccountId descending
                                         select account.AccountId)
                        .Take(1)
                        .Select(accountId => "L" + (int.Parse(accountId.Substring(1)) + 1).ToString().PadLeft(accountId.Substring(1).Length, '0'))
                        .FirstOrDefault();

                    return Json(new { nextAccountId });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetMaxLedgerAccountID: {ex.Message}");
                    return StatusCode(500, new { message = "An error occurred while fetching data.", error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet("FindDuplicatesOnAccountHead")]
        public async Task<ActionResult> FindDuplicatesOnAccountHead(string stAccountHeadValue)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var result = (from c in dbContext.Tbl201ChartOfAccounts
                              where c.AccountHead.ToUpper() == stAccountHeadValue.ToUpper()
                              group c by c.AccountHead into g
                              select new
                              {
                                  MaxAccountId = g.Max(x => x.AccountId),
                                  IsValid = g.Count() > 0,
                                  AccountHead = g.Key
                              }).ToList();

                return Json(result);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet("GetAttachments")]
        public async Task<ActionResult> GetAttachments()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                string LedgerNo = "L00506";
                var query = from a in dbContext.Tbl20116LedgerDocuments
                            join b in dbContext.Tbl101DocumentTypes on a.DocumentType equals b.DocumentTypeId
                            join c in dbContext.Tbl201ChartOfAccounts on a.LedgerNo equals c.AccountId
                            where c.AccountId == LedgerNo
                            select new
                            {
                                b.DocumentType,
                                a.DocumentRefNo,
                                a.DocumentExpDate,
                                a.DocumentNotificationDate
                            };

                return Json(query);
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost("InsertLedgerAccount")]
        public async Task<ActionResult> InsertLedgerAccount(string AccountID, string RefNo)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var LedgerAccuntEntry = new Tbl201ChartOfAccount
                {
                    AccountId = AccountID,
                    AccountGroupId = RefNo
                };

                dbContext.Tbl201ChartOfAccounts.Add(LedgerAccuntEntry);
                await dbContext.SaveChangesAsync();

                return Json(new { success = true, message = "Ledger account inserted successfully." });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }










        //[HttpPost("Insert")]
        //public IActionResult Insert([FromBody] Tbl201ChartOfAccount chartAccount)
        //{
        //	if (chartAccount == null)
        //	{
        //		return BadRequest("Invalid data.");
        //	}

        //	//var LedgerAccountEntries = _context.Tbl201ChartOfAccounts.Add(chartAccount);

        //	//_context.SaveChanges();

        //	var LedgerAccuntEntry = new Tbl201ChartOfAccount
        //	{
        //		AccountId = chartAccount.AccountId,
        //		ReferenceNo = chartAccount.ReferenceNo,
        //		AccountHead = chartAccount.AccountHead,
        //		AccountGroupId = chartAccount.AccountGroupId,
        //		OpeningBalance = chartAccount.OpeningBalance,
        //		OpeningBalanceDrCr = chartAccount.OpeningBalanceDrCr,
        //		IsDefaultForCash = chartAccount.IsDefaultForCash,
        //		VoucherAbbr = chartAccount.VoucherAbbr,
        //		IsMaintainBillByBill = chartAccount.IsMaintainBillByBill,
        //		NoOfDaysCreditPeriod = chartAccount.NoOfDaysCreditPeriod,
        //		IsEmployeeAllocated = chartAccount.IsEmployeeAllocated,
        //		IsPropertyAllocated = chartAccount.IsPropertyAllocated,
        //		IsEmployeePaymentAc = chartAccount.IsEmployeePaymentAc,
        //		IsLedgerObselete = chartAccount.IsLedgerObselete,
        //		//ContactDetails
        //		AccountBranch = chartAccount.AccountBranch,
        //		AccountSubGroup = chartAccount.AccountSubGroup,
        //		SalesPersonCode = chartAccount.SalesPersonCode,
        //		AccountsContactName = chartAccount.AccountsContactName,
        //		AccountsContactTitle = chartAccount.AccountsContactTitle,
        //		AccountsContactMobile = chartAccount.AccountsContactMobile,
        //		EmailAddress = chartAccount.EmailAddress,
        //		LedgerRemarks = chartAccount.LedgerRemarks,
        //		//BankDetails
        //		BankAccountName = chartAccount.BankAccountName,
        //		BankName = chartAccount.BankName,
        //		BankBranch = chartAccount.BankBranch,
        //		BankAccountNo = chartAccount.BankAccountNo,
        //		BankIban = chartAccount.BankIban,
        //		BankSwiftCode = chartAccount.BankSwiftCode,
        //		//ClientDetails
        //		BillingName = chartAccount.BillingName,
        //		VatregistrationNo = chartAccount.VatregistrationNo,
        //		ClientGroupVatnumber = chartAccount.ClientGroupVatnumber,
        //		ClientOtherIdtype = chartAccount.ClientOtherIdtype,
        //		ClientOtherId = chartAccount.ClientOtherId,
        //		ClientAddressStreet = chartAccount.ClientAddressStreet,
        //		ClientAdditionalStreet = chartAccount.ClientAdditionalStreet,
        //		ClientBuildingNumber = chartAccount.ClientBuildingNumber,
        //		ClientCity = chartAccount.ClientCity,
        //		ClientProvince = chartAccount.ClientProvince,
        //		ClientPostalCode = chartAccount.ClientPostalCode,
        //		ClientNeighborhood = chartAccount.ClientNeighborhood,
        //		//ClientCountryCode = chartAccount.ClientCountryCode,
        //		//Supplier Details
        //		SupplierName = chartAccount.SupplierName,
        //		SupplierVatno = chartAccount.SupplierVatno,
        //		SupplierGroupVatnumber = chartAccount.SupplierGroupVatnumber,
        //		//SupplierOtherIdtype = chartAccount.SupplierOtherIdtype,
        //		//SupplierOtherId = chartAccount.SupplierOtherId,
        //		SupplierAddressStreet = chartAccount.SupplierAddressStreet,
        //		SupplierAdditionalStreet = chartAccount.SupplierAdditionalStreet,
        //		SupplierBuildingNumber = chartAccount.SupplierBuildingNumber,
        //		SupplierCity = chartAccount.SupplierCity,
        //		SupplierProvince = chartAccount.SupplierProvince,
        //		SupplierPostalCode = chartAccount.SupplierPostalCode,
        //		SupplierNeighborhood = chartAccount.SupplierNeighborhood,
        //		//SupplierCountryCode = chartAccount.SupplierCountryCode,
        //		//Billing Details
        //		//BillingName = chartAccount.BillingNameAr
        //		BillingAddress = chartAccount.BillingAddress,
        //		ClientTin = chartAccount.ClientTin,
        //		BillingContactPerson = chartAccount.BillingContactPerson,
        //		BillingPhoneNo = chartAccount.BillingPhoneNo,
        //		BillingFaxNo = chartAccount.BillingFaxNo,
        //		ClientVendorNo = chartAccount.ClientVendorNo,
        //		BillingContactPersonTitle = chartAccount.BillingContactPersonTitle,
        //		BillingBankAccount = chartAccount.BillingBankAccount,
        //		//Obselete details
        //		SupplierAddress = chartAccount.SupplierAddress
        //	};

        //	var LedgerAccountEntries = _context.Tbl201ChartOfAccounts.Add(LedgerAccuntEntry);

        //	_context.SaveChanges();

        //	// You can now work with the 'person' object, which is deserialized from the JSON.
        //	// For example, log or return the data:
        //	return Json(new { success = true, message = "Ledger account inserted successfully" });
        //}
        [HttpPost("InsertOpeningBalance")]
        public async Task<IActionResult> InsertOpeningBalance([FromBody] Tbl201ChartOfAccount chartAccount)
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    return StatusCode(500, new { success = false, message = "Tenant context could not be determined." });
                }

                if (chartAccount == null)
                    return BadRequest("Invalid data.");

                if (chartAccount.AccountId == null || chartAccount.OpeningBalance == null || chartAccount.OpeningBalanceDrCr == null)
                    return BadRequest("Missing required fields.");

                string accountID = chartAccount.AccountId;
                decimal? openingBalance = chartAccount.OpeningBalance;
                string openingBalanceDrCr = chartAccount.OpeningBalanceDrCr;

                string debitAccount = accountID.Replace("SYS-OP-", "");
                string creditAccount = "L00503";
                var todayDate = DateTime.Now;

                var accountBooksOpeningDate = await dbContext.Tbl901CompanyDetails02s
                                                            .Where(c => c.CompanyId == 1)
                                                            .Select(c => c.AccountBooksOpeningDate)
                                                            .FirstOrDefaultAsync();

                if (accountBooksOpeningDate == null)
                    return NotFound("AccountBooksOpeningDate not found.");

                var voucherDate = accountBooksOpeningDate.Value.AddDays(-1);

                // ✅ Check if VoucherNo already exists
                var existingVoucher = await dbContext.Tbl201VoucherEntries
                                                     .FirstOrDefaultAsync(v => v.VoucherNo == accountID);

                if (existingVoucher != null)
                {
                    // ✅ Update existing record
                    existingVoucher.VoucherAmount = openingBalance;
                   

                    await dbContext.SaveChangesAsync();

                    return Json(new { success = true, message = "Opening balance updated successfully." });
                }
                else
                {
                    // ✅ Insert using stored procedure inside transaction
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            await dbContext.Database.ExecuteSqlRawAsync(
                                "EXEC sp20115InsertOpeningBalanceVoucher @VoucherNo, @VoucherDate, @DebitAccount, @CreditAccount, @AddedBy, @AddedOn, @TotalAmount",
                                new SqlParameter("@VoucherNo", accountID),
                                new SqlParameter("@VoucherDate", voucherDate),
                                new SqlParameter("@DebitAccount", debitAccount),
                                new SqlParameter("@CreditAccount", creditAccount),
                                new SqlParameter("@AddedBy", "admin"),
                                new SqlParameter("@AddedOn", todayDate),
                                new SqlParameter("@TotalAmount", openingBalance)
                            );

                            await transaction.CommitAsync();
                            return Json(new { success = true, message = "Opening balance voucher inserted successfully." });
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            return StatusCode(500, new { success = false, message = "An error occurred during insertion: " + ex.Message });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "An unexpected error occurred: " + ex.Message });
            }
        }


        [HttpPost("InsertOrUpdate")]
        public async Task<IActionResult> InsertOrUpdateAsync([FromBody] Tbl201ChartOfAccount chartAccount, string AccountGroupID)
        {
            if (chartAccount == null)
            {
                return BadRequest("Invalid data.");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var addedBy = HttpContext.Session.GetString("UserName") ?? "System";
                    var addedOn = DateTime.Now;
                    var existingAccount = dbContext.Tbl201ChartOfAccounts
                        .FirstOrDefault(a => a.AccountId == chartAccount.AccountId);

                    if (existingAccount != null)
                    {
                        existingAccount.ReferenceNo = chartAccount.ReferenceNo;
                        existingAccount.AccountHead = chartAccount.AccountHead;
                        existingAccount.AccountGroupId = AccountGroupID;
                        existingAccount.OpeningBalance = chartAccount.OpeningBalance;
                        existingAccount.OpeningBalanceDrCr = chartAccount.OpeningBalanceDrCr;
                        existingAccount.IsDefaultForCash = chartAccount.IsDefaultForCash;
                        existingAccount.VoucherAbbr = chartAccount.VoucherAbbr;
                        existingAccount.IsMaintainBillByBill = chartAccount.IsMaintainBillByBill;
                        existingAccount.NoOfDaysCreditPeriod = chartAccount.NoOfDaysCreditPeriod;
                        existingAccount.IsEmployeeAllocated = chartAccount.IsEmployeeAllocated;
                        existingAccount.IsPropertyAllocated = chartAccount.IsPropertyAllocated;
                        existingAccount.IsEmployeePaymentAc = chartAccount.IsEmployeePaymentAc;
                        existingAccount.IsLedgerObselete = chartAccount.IsLedgerObselete;
                        existingAccount.AccountBranch = chartAccount.AccountBranch;
                        existingAccount.AccountSubGroup = chartAccount.AccountSubGroup;
                        existingAccount.SalesPersonCode = chartAccount.SalesPersonCode;
                        existingAccount.AccountsContactName = chartAccount.AccountsContactName;
                        existingAccount.AccountsContactTitle = chartAccount.AccountsContactTitle;
                        existingAccount.AccountsContactMobile = chartAccount.AccountsContactMobile;
                        existingAccount.EmailAddress = chartAccount.EmailAddress;
                        existingAccount.LedgerRemarks = chartAccount.LedgerRemarks;
                        existingAccount.BankAccountName = chartAccount.BankAccountName;
                        existingAccount.AccountHeadArabic = chartAccount.AccountHeadArabic;
                        existingAccount.BankName = chartAccount.BankName;
                        existingAccount.BankNameAr = chartAccount.BankNameAr;
                        existingAccount.BankAccountNameAr = chartAccount.BankAccountNameAr;
                        existingAccount.BankBranch = chartAccount.BankBranch;
                        existingAccount.BankBranchAr = chartAccount.BankBranchAr;
                        existingAccount.BankAccountNo = chartAccount.BankAccountNo;
                        existingAccount.BankIban = chartAccount.BankIban;
                        existingAccount.BankSwiftCode = chartAccount.BankSwiftCode;
                        existingAccount.BillingName = chartAccount.BillingName;
                        existingAccount.BillingNameAr = chartAccount.BillingNameAr;
                        existingAccount.VatregistrationNo = chartAccount.VatregistrationNo;
                        existingAccount.ClientGroupVatnumber = chartAccount.ClientGroupVatnumber;
                        existingAccount.ClientOtherIdtype = chartAccount.ClientOtherIdtype;
                        existingAccount.ClientOtherId = chartAccount.ClientOtherId;
                        existingAccount.ClientAddressStreet = chartAccount.ClientAddressStreet;
                        existingAccount.ClientAddressStreetAr = chartAccount.ClientAddressStreetAr;
                        existingAccount.ClientAdditionalStreet = chartAccount.ClientAdditionalStreet;
                        existingAccount.ClientAdditionalStreetAr = chartAccount.ClientAdditionalStreetAr;
                        existingAccount.ClientBuildingNumber = chartAccount.ClientBuildingNumber;
                        existingAccount.ClientCity = chartAccount.ClientCity;
                        existingAccount.ClientCityAr = chartAccount.ClientCityAr;
                        existingAccount.ClientAdditionalNumber = chartAccount.ClientAdditionalNumber;
                        existingAccount.ClientProvince = chartAccount.ClientProvince;
                        existingAccount.ClientProvinceAr = chartAccount.ClientProvinceAr;
                        existingAccount.ClientPostalCode = chartAccount.ClientPostalCode;
                        existingAccount.ClientNeighborhood = chartAccount.ClientNeighborhood;
                        existingAccount.ClientNeighborhoodAr = chartAccount.ClientNeighborhoodAr;
                        existingAccount.ClientCountryCode = chartAccount.ClientCountryCode;
                        existingAccount.SupplierName = chartAccount.SupplierName;
                        existingAccount.SupplierNameAr = chartAccount.SupplierNameAr;
                        existingAccount.SupplierVatno = chartAccount.SupplierVatno;
                        existingAccount.SupplierGroupVatnumber = chartAccount.SupplierGroupVatnumber;
                        existingAccount.SupplierOtherIdtype = chartAccount.SupplierOtherIdtype;
                        existingAccount.SupplierOtherId = chartAccount.SupplierOtherId;
                        existingAccount.SupplierAddressStreet = chartAccount.SupplierAddressStreet;
                        existingAccount.SupplierAddressStreetAr = chartAccount.SupplierAddressStreetAr;
                        existingAccount.SupplierAdditionalStreet = chartAccount.SupplierAdditionalStreet;
                        existingAccount.SupplierAdditionalStreetAr = chartAccount.SupplierAdditionalStreetAr;
                        existingAccount.SupplierBuildingNumber = chartAccount.SupplierBuildingNumber;
                        existingAccount.SupplierCity = chartAccount.SupplierCity;
                        existingAccount.SupplierCityAr = chartAccount.SupplierCityAr;
                        existingAccount.SupplierAdditionalNumber = chartAccount.SupplierAdditionalNumber;
                        existingAccount.SupplierProvince = chartAccount.SupplierProvince;
                        existingAccount.SupplierProvinceAr = chartAccount.SupplierProvinceAr;
                        existingAccount.SupplierPostalCode = chartAccount.SupplierPostalCode;
                        existingAccount.SupplierNeighborhood = chartAccount.SupplierNeighborhood;
                        existingAccount.SupplierNeighborhoodAr = chartAccount.SupplierNeighborhoodAr;
                        existingAccount.SupplierCountryCode = chartAccount.SupplierCountryCode;
                        existingAccount.BillingAddress = chartAccount.BillingAddress;
                        existingAccount.BillingAddressAr = chartAccount.BillingAddressAr;
                        existingAccount.ClientTin = chartAccount.ClientTin;
                        existingAccount.BillingContactPerson = chartAccount.BillingContactPerson;
                        existingAccount.BillingPhoneNo = chartAccount.BillingPhoneNo;
                        existingAccount.BillingFaxNo = chartAccount.BillingFaxNo;
                        existingAccount.ClientVendorNo = chartAccount.ClientVendorNo;
                        existingAccount.BillingContactPersonTitle = chartAccount.BillingContactPersonTitle;
                        existingAccount.BillingBankAccount = chartAccount.BillingBankAccount;
                        existingAccount.SupplierAddress = chartAccount.SupplierAddress;
                        existingAccount.RecordModifiedBy = addedBy;
                        existingAccount.RecordModifiedOn = addedOn;


                        dbContext.Tbl201ChartOfAccounts.Update(existingAccount);
                        dbContext.SaveChanges();
                        await _userActionLogger.LogAsync(
                        module: "Finance > Ledger Account",
                        actionDetail: $"Updated Ledger: {chartAccount.AccountId}",
                        documentNo: chartAccount.AccountId
                        );
                        return Json(new { success = true, modifiedby = existingAccount.RecordModifiedBy, modifiedon = existingAccount.RecordModifiedOn?.ToString("yyyy-MM-dd"), message = "Ledger account updated successfully" });
                    }
                    else
                    {

                        var newAccount = new Tbl201ChartOfAccount
                        {
                            AccountId = chartAccount.AccountId,
                            ReferenceNo = chartAccount.ReferenceNo,
                            AccountHead = chartAccount.AccountHead,
                            AccountGroupId = chartAccount.AccountGroupId,
                            OpeningBalance = chartAccount.OpeningBalance,
                            OpeningBalanceDrCr = chartAccount.OpeningBalanceDrCr,
                            IsDefaultForCash = chartAccount.IsDefaultForCash,
                            VoucherAbbr = chartAccount.VoucherAbbr,
                            IsMaintainBillByBill = chartAccount.IsMaintainBillByBill,
                            NoOfDaysCreditPeriod = chartAccount.NoOfDaysCreditPeriod,
                            IsEmployeeAllocated = chartAccount.IsEmployeeAllocated,
                            IsPropertyAllocated = chartAccount.IsPropertyAllocated,
                            IsEmployeePaymentAc = chartAccount.IsEmployeePaymentAc,
                            IsLedgerObselete = chartAccount.IsLedgerObselete,
                            AccountBranch = chartAccount.AccountBranch,
                            AccountSubGroup = chartAccount.AccountSubGroup,
                            SalesPersonCode = chartAccount.SalesPersonCode,
                            AccountsContactName = chartAccount.AccountsContactName,
                            AccountsContactTitle = chartAccount.AccountsContactTitle,
                            AccountsContactMobile = chartAccount.AccountsContactMobile,
                            EmailAddress = chartAccount.EmailAddress,
                            LedgerRemarks = chartAccount.LedgerRemarks,
                            BankAccountName = chartAccount.BankAccountName,
                            BankAccountNameAr = chartAccount.BankAccountNameAr,
                            AccountHeadArabic = chartAccount.AccountHeadArabic,
                            BankName = chartAccount.BankName,
                            BankNameAr = chartAccount.BankNameAr,
                            BankBranch = chartAccount.BankBranch,
                            BankBranchAr = chartAccount.BankBranchAr,
                            BankAccountNo = chartAccount.BankAccountNo,
                            BankIban = chartAccount.BankIban,
                            BankSwiftCode = chartAccount.BankSwiftCode,
                            BillingName = chartAccount.BillingName,
                            BillingNameAr = chartAccount.BillingNameAr,
                            VatregistrationNo = chartAccount.VatregistrationNo,
                            ClientGroupVatnumber = chartAccount.ClientGroupVatnumber,
                            ClientOtherIdtype = chartAccount.ClientOtherIdtype,
                            ClientOtherId = chartAccount.ClientOtherId,
                            ClientAddressStreet = chartAccount.ClientAddressStreet,
                            ClientAddressStreetAr = chartAccount.ClientAddressStreetAr,
                            ClientAdditionalStreet = chartAccount.ClientAdditionalStreet,
                            ClientAdditionalStreetAr = chartAccount.ClientAdditionalStreetAr,
                            ClientBuildingNumber = chartAccount.ClientBuildingNumber,
                            ClientCity = chartAccount.ClientCity,
                            ClientCityAr = chartAccount.ClientCityAr,
                            ClientAdditionalNumber = chartAccount.ClientAdditionalNumber,
                            ClientProvince = chartAccount.ClientProvince,
                            ClientProvinceAr = chartAccount.ClientProvinceAr,
                            ClientPostalCode = chartAccount.ClientPostalCode,
                            ClientNeighborhood = chartAccount.ClientNeighborhood,
                            ClientNeighborhoodAr = chartAccount.ClientNeighborhoodAr,
                            ClientCountryCode = chartAccount.ClientCountryCode,
                            SupplierName = chartAccount.SupplierName,
                            SupplierNameAr = chartAccount.SupplierNameAr,
                            SupplierVatno = chartAccount.SupplierVatno,
                            SupplierGroupVatnumber = chartAccount.SupplierGroupVatnumber,
                            SupplierOtherIdtype = chartAccount.SupplierOtherIdtype,
                            SupplierOtherId = chartAccount.SupplierOtherId,
                            SupplierAddressStreet = chartAccount.SupplierAddressStreet,
                            SupplierAddressStreetAr = chartAccount.SupplierAddressStreetAr,
                            SupplierAdditionalStreet = chartAccount.SupplierAdditionalStreet,
                            SupplierAdditionalStreetAr = chartAccount.SupplierAdditionalStreetAr,
                            SupplierBuildingNumber = chartAccount.SupplierBuildingNumber,
                            SupplierCity = chartAccount.SupplierCity,
                            SupplierCityAr = chartAccount.SupplierCityAr,
                            SupplierAdditionalNumber = chartAccount.SupplierAdditionalNumber,
                            SupplierProvince = chartAccount.SupplierProvince,
                            SupplierProvinceAr = chartAccount.SupplierProvinceAr,
                            SupplierPostalCode = chartAccount.SupplierPostalCode,
                            SupplierNeighborhood = chartAccount.SupplierNeighborhood,
                            SupplierNeighborhoodAr = chartAccount.SupplierNeighborhoodAr,
                            SupplierCountryCode = chartAccount.SupplierCountryCode,
                            BillingAddress = chartAccount.BillingAddress,
                            BillingAddressAr = chartAccount.BillingAddressAr,
                            ClientTin = chartAccount.ClientTin,
                            BillingContactPerson = chartAccount.BillingContactPerson,
                            BillingPhoneNo = chartAccount.BillingPhoneNo,
                            BillingFaxNo = chartAccount.BillingFaxNo,
                            ClientVendorNo = chartAccount.ClientVendorNo,
                            BillingContactPersonTitle = chartAccount.BillingContactPersonTitle,
                            BillingBankAccount = chartAccount.BillingBankAccount,
                            SupplierAddress = chartAccount.SupplierAddress,
                            RecordCreatedBy = addedBy,
                            RecordCreatedOn = addedOn
                        };



                        var newAssetMaster = new Tbl20105AssetMaster
                        {
                            AssetLedgerNo = chartAccount.AccountId,
                            
                        };

                  

                        dbContext.Tbl201ChartOfAccounts.Add(newAccount);
                        dbContext.Tbl20105AssetMasters.Add(newAssetMaster);
                        dbContext.SaveChanges();
                        await _userActionLogger.LogAsync(
                       module: "Finance > Ledger Account",
                       actionDetail: $"Saved Ledger: {chartAccount.AccountId}",
                       documentNo: chartAccount.AccountId
                       );
                        return Json(new { success = true, createdBy = newAccount.RecordCreatedBy, createdon= newAccount.RecordCreatedOn?.ToString("yyyy-MM-dd"), message = "Account Ledger Information Saved Successfully" });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in InsertOrUpdate: {ex.Message}");
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet("GetAccountDetails")]
        public async Task<ActionResult> GetAccountDetails(string accountID)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    var userName = HttpContext.Session.GetString("UserName");
                    if (string.IsNullOrEmpty(accountID))
                    {
                        return BadRequest(new { Message = "AccountID cannot be null or empty." });
                    }

                    var accountDetails = await dbContext.Tbl201ChartOfAccounts
                        .Where(p => p.AccountId == accountID)
                        .Select(i => new
                        {
                            i.AccountId,
                            i.ReferenceNo,
                            i.AccountHead,
                            i.AccountGroupId,
                            i.OpeningBalance,
                            i.OpeningBalanceDrCr,
                            i.IsDefaultForCash,
                            i.VoucherAbbr,
                            i.IsMaintainBillByBill,
                            i.NoOfDaysCreditPeriod,
                            i.IsEmployeeAllocated,
                            i.IsPropertyAllocated,
                            i.IsEmployeePaymentAc,
                            i.IsLedgerObselete,
                            i.AccountBranch,
                            i.AccountSubGroup,
                            i.SalesPersonCode,
                            i.AccountsContactName,
                            i.AccountsContactTitle,
                            i.AccountsContactMobile,
                            i.EmailAddress,
                            i.LedgerRemarks,
                            i.BankAccountName,
                            i.BankAccountNameAr,
                            i.AccountHeadArabic,
                            i.BankName,
                            i.BankNameAr,
                            i.BankBranch,
                            i.BankBranchAr,
                            i.BankAccountNo,
                            i.BankIban,
                            i.BankSwiftCode,
                            i.BillingName,
                            i.BillingNameAr,
                            i.VatregistrationNo,
                            i.ClientGroupVatnumber,
                            i.ClientOtherIdtype,
                            i.ClientOtherId,
                            i.ClientAddressStreet,
                            i.ClientAddressStreetAr,
                            i.ClientAdditionalStreet,
                            i.ClientAdditionalStreetAr,
                            i.ClientBuildingNumber,
                            i.ClientCity,
                            i.ClientCityAr,
                            i.ClientAdditionalNumber,
                            i.ClientProvince,
                            i.ClientProvinceAr,
                            i.ClientPostalCode,
                            i.ClientNeighborhood,
                            i.ClientNeighborhoodAr,
                            i.SupplierName,
                            i.SupplierNameAr,
                            i.SupplierVatno,
                            i.SupplierGroupVatnumber,
                            i.SupplierAddressStreet,
                            i.SupplierAddressStreetAr,
                            i.SupplierAdditionalStreet,
                            i.SupplierAdditionalStreetAr,
                            i.SupplierBuildingNumber,
                            i.SupplierCity,
                            i.SupplierCityAr,
                            i.SupplierAdditionalNumber,
                            i.SupplierProvince,
                            i.SupplierProvinceAr,
                            i.SupplierPostalCode,
                            i.SupplierNeighborhood,
                            i.SupplierNeighborhoodAr,
                            i.BillingAddress,
                            i.BillingAddressAr,
                            i.ClientTin,
                            i.BillingContactPerson,
                            i.BillingPhoneNo,
                            i.BillingFaxNo,
                            i.ClientVendorNo,
                            i.BillingContactPersonTitle,
                            i.BillingBankAccount,
                            i.SupplierAddress,
                            i.SupplierOtherId,
                            i.SupplierOtherIdtype,
                            i.SupplierCountryCode,
                            i.ClientCountryCode,
                            i.RecordCreatedBy,
                            i.RecordCreatedOn,
                            i.RecordModifiedBy,
                            i.RecordModifiedOn


                        })
                        .ToListAsync();

                    if (!accountDetails.Any())
                    {
                        return NotFound(new { Message = "No account details found for the provided AccountID." });
                    }

                    return Json(new { UserName = userName, accountDetails });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in GetAccountDetails: {ex.Message}");
                    return StatusCode(500, new { Message = "An unexpected error occurred.", Error = ex.Message });
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
        [HttpGet("GetCreditPeriod")]
        public IActionResult GetCreditPeriod(string accountId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))

            {
                try
                {
                    var creditPeriod = dbContext.Tbl201ChartOfAccounts
                .Where(a => a.AccountId == accountId)
                .Select(a => a.NoOfDaysCreditPeriod)
                .FirstOrDefault();

                    return Json(new { noOfDays = creditPeriod });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { Message = "An unexpected error occurred.", Error = ex.Message });
                }

            }


            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}



















