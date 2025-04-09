using QD.ERP.Web.DAL.Entities;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

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
                using (dbContext)
                {
                    return dbContext.TblUserAccesses
                                    .Where(ua => ua.UserId == userId)
                                    .Select(ua => ua.Module)
                                    .Distinct()
                                    .ToList();
                }
            }
            return new List<string>();
        }
    }
}
