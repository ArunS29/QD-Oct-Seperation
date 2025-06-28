using QD.ERP.Web.DAL.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace QD.ERP.Web.Service
{
    public class UserAccessService
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<UserAccessService> _logger;

        public UserAccessService(ILogger<UserAccessService> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        public List<string> GetUserModules(int userId)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                // Do not dispose dbContext if managed by DI
                return dbContext.TblUserAccessWebs
                                .Where(ua => ua.UserId == userId && ua.ItemForm == "ERP Module Access" && ua.ItemVisible == true)
                                .Select(ua => ua.Module)
                                .Distinct()
                                .ToList();
            }
            return new List<string>();
        }

        public class UserMenuAccess
        {
            public string ItemDescription { get; set; }
            public string ItemName { get; set; }
            public bool ItemEnabled { get; set; }
            public bool ItemVisible { get; set; }
        }

        public async Task<List<UserMenuAccess>> GetFinanceMenuAccessAsync(int userId1)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                // Corrected: "Finance Menu" and property initializers
                var result = await dbContext.TblUserAccessWebs
                    .Where(x => x.UserId == userId1 && x.ItemForm == "Finance Menu")
                    .Select(x => new UserMenuAccess
                    {
                        ItemDescription = x.ItemDescription,
                        ItemName = x.ItemName,
                        ItemEnabled = x.ItemEnabled == true,
                        ItemVisible = x.ItemVisible == true
                    })
                    .ToListAsync();

                return result;
            }
            return new List<UserMenuAccess>();
        }

        public async Task<List<UserMenuAccess>> GetInventoryMenuAccessAsync(int userId1)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                // Corrected: "Finance Menu" and property initializers
                var result = await dbContext.TblUserAccessWebs
                    .Where(x => x.UserId == userId1 && x.ItemForm == "Inventory Menu")
                    .Select(x => new UserMenuAccess
                    {
                        ItemDescription = x.ItemDescription,
                        ItemEnabled = x.ItemEnabled == true,
                        ItemVisible = x.ItemVisible == true
                    })
                    .ToListAsync();

                return result;
            }
            return new List<UserMenuAccess>();
        }
        public async Task<List<UserMenuAccess>> GetEquipmentMenuAccessAsync(int userId1)
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                // Corrected: "Finance Menu" and property initializers
                var result = await dbContext.TblUserAccessWebs
                    .Where(x => x.UserId == userId1 && x.ItemForm == "Equipment Rental Menu")
                    .Select(x => new UserMenuAccess
                    {
                        ItemDescription = x.ItemDescription,
                        ItemEnabled = x.ItemEnabled == true,
                        ItemVisible = x.ItemVisible == true
                    })
                    .ToListAsync();

                return result;
            }
            return new List<UserMenuAccess>();
        }
    }
}
