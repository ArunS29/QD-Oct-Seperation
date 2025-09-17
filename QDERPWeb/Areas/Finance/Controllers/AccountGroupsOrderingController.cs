using DevExtreme.AspNet.Data;
using DevExtreme.AspNet.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using QD.ERP.Web.Areas.Finance.Models;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace QD.ERP.Web.Areas.Finance.Controllers
{
    [Route("/api/[controller]/[action]")]
    [ApiController]
    public class AccountGroupsOrderingController : Controller
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<AccountGroupsOrderingController> _logger;

        public AccountGroupsOrderingController(ILogger<AccountGroupsOrderingController> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccountGroupsOrdering(DataSourceLoadOptions loadOptions)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                var qry20164salarypayableledgermaster = dbContext.Qry201207accountGroupOrderings
    .Select(i => new
    {
        i.ChartOfAccountsOrder,
        i.MasterGroup,
        i.AccountGroupId,
        i.AccountGroup,
        i.AccountGroupOrderNo
    })
    .OrderBy(i => i.ChartOfAccountsOrder)   // First order by ChartOfAccountsOrder
    .ThenBy(i => i.AccountGroupOrderNo);  // Then order by AccountGroupOrderNo

                var resultList1 = await qry20164salarypayableledgermaster.ToListAsync();
                return Json(DataSourceLoader.Load(resultList1.AsQueryable(), loadOptions));
                
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }

        [HttpPost]
        public IActionResult UpdateAccountGroupOrder([FromBody] List<Qry201207accountGroupOrdering> updatedData)
        {
            if (updatedData == null || updatedData.Count == 0)
            {
                return BadRequest("No data received");
            }

            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                foreach (var item in updatedData)
                {
                    var existingRecord = dbContext.Tbl201AccountGroups
                        .FirstOrDefault(x => x.AccountGroupId == item.AccountGroupId);

                    if (existingRecord != null)
                    {
                        existingRecord.AccountGroupOrderNo = item.AccountGroupOrderNo;
                        dbContext.Tbl201AccountGroups.Update(existingRecord);
                    }
                }

                dbContext.SaveChanges(); // Save changes to the database

                return Ok(new { message = "Data updated successfully" });
            }

            return Unauthorized(new { message = "Invalid tenant.", success = false });
        }
    }
}
