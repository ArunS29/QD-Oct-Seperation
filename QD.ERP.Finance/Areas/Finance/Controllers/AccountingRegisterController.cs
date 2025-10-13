using QD.ERP.Shared.DAL.Entities;
using QDERPWeb.Models;
using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using QD.ERP.Shared.Service;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.Extensions.Logging;

namespace QD.ERP.Finance.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AccountingRegisterController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<AccountingRegisterController> _logger;

        public AccountingRegisterController(ILogger<AccountingRegisterController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetVoucherTypes(DataSourceLoadOptions loadOptions)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    var voucherTypelists = dbContext.Tbl201VoucherTypes.Select(i => new
                    {
                        i.VoucherTypeId,
                        i.VoucherType,
                        i.VoucherTypeAr
                    });

                    return Json(await DataSourceLoader.LoadAsync(voucherTypelists, loadOptions));
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetVoucherTypes: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetVouchers(string voucherType, string frmDate, string toDate)
        {
            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
                {
                    DateTime from = new DateTime(2000, 1, 1);
                    DateTime to = DateTime.Now;

                    if (!string.IsNullOrEmpty(frmDate) && DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedFrom))
                    {
                        from = parsedFrom;
                    }

                    if (!string.IsNullOrEmpty(toDate) && DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedTo))
                    {
                        to = parsedTo;
                    }

                    var procedures = new ERPMasterWtDataContextProcedures(dbContext);
                    var ledgerData = await dbContext.AccountRegisters
                       .FromSqlRaw("EXEC StProAccountLedgerByVoucherType @p0, @p1, @p2", voucherType, from, to)
                       .ToListAsync();

                   // var ledgerData = await procedures.StProAccountLedgerByVoucherTypeAsync(string.IsNullOrEmpty(voucherType) ? null : voucherType, from, to);

                    var sortedLedgerData = ledgerData.OrderBy(x => x.VoucherDate).ToList();

                    return Json(sortedLedgerData);
                }

                return Unauthorized(new { message = "Invalid tenant.", success = false });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetVouchers: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
