using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;


namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("api/[controller]/[action]")]
    public class AccountingLedgersController : Controller
    {
        private readonly IMemoryCache _cache;
        private readonly DbContextFactory _dbContextFactory;
        private readonly IConfiguration _configuration;
        private readonly ERPMasterWtDataContext _context;

        private ILogger<AccountingLedgersController> _logger;
        public AccountingLedgersController(ILogger<AccountingLedgersController> logger, IMemoryCache cache, DbContextFactory dbContextFactory, IConfiguration configuration)
        {
            _cache = cache;
            _dbContextFactory = dbContextFactory;
            _configuration = configuration;
            _logger = logger;
        }


        private bool TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext)
        {
            tenant = null;
            dbContext = null;

            var tenantName = HttpContext.Session.GetString("TenantName");
            if (string.IsNullOrEmpty(tenantName))
            {
                return false;
            }

            if (_cache.TryGetValue("tenant_", out Dictionary<string, Tenant> tenantCache) &&
                tenantCache.TryGetValue(tenantName.ToLower(), out tenant))
            {
                try
                {
                    dbContext = _dbContextFactory.CreateDbContext(tenant.ConnectionString);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }

            return false;
        }

        [HttpGet]
        public async Task<ActionResult> GetLedgerAccounts(DataSourceLoadOptions loadOptions)
        {
            if (TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    var ledgerAccounts = dbContext.Qry201ListOfAccounts
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

                    return Json(await DataSourceLoader.LoadAsync(ledgerAccounts, loadOptions));
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public async Task<ActionResult> GetVouchers(string accountId, string frmDate, string toDate)
        {
            if (TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (!DateTime.TryParseExact(frmDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime from))
                        return BadRequest("Invalid from date format. Use MM/dd/yyyy.");

                    if (!DateTime.TryParseExact(toDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime to))
                        return BadRequest("Invalid to date format. Use MM/dd/yyyy.");

                    var ledgerData = await dbContext.AccountLedgers
                        .FromSqlRaw("EXEC StProAccountLedger @p0, @p1, @p2", accountId, from, to)
                        .ToListAsync();

                    return Json(ledgerData);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpGet]
        public IActionResult GetAccountId(string id)
        {
            if (TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                try
                {
                    if (string.IsNullOrEmpty(id))
                        return Json(new { success = false, message = "Invalid Cost Center ID." });

                    var costCenter = dbContext.Tbl201ChartOfAccounts.FirstOrDefault(c => c.AccountId == id);
                    if (costCenter == null)
                        return Json(new { success = false, message = "Cost Center not found." });

                    return Json(new { success = true, data = costCenter });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = ex.Message });
                    _logger.LogError("This is an error log message from the AccountingLedger controller from GetAccountId(string id) at {DateTime}.");
                }
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}
