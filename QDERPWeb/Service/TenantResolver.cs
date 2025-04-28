using QD.ERP.Web.Models.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SaasKit.Multitenancy;
using QD.ERP.Web.Models.DALCommon;

namespace QD.ERP.Web.Service
{
    public class TenantResolver : ITenantResolver<Tenant>
    {
        private readonly ERPCommonContext _dbContext;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "tenant_"; // Prefix for cache key

public TenantResolver(IMemoryCache cache, ERPCommonContext dbContext)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        public Task<TenantContext<Tenant>> ResolveAsync(HttpContext context)
        {
            // Extract tenant name from route (e.g., /tenantA/... -> tenantName = "tenantA")
            var tenantName = context.Items["TenantName"]?.ToString();

     if (string.IsNullOrEmpty(tenantName))
            {
                return Task.FromResult<TenantContext<Tenant>>(null);
            }

            // Retrieve the tenant cache (dictionary of tenants)
            if (!_cache.TryGetValue(CacheKey, out Dictionary<string, Tenant> tenantCache))
            {
                // If the tenant cache doesn't exist, create a new one
                tenantCache = new Dictionary<string, Tenant>();
                _cache.Set(CacheKey, tenantCache, TimeSpan.FromHours(1));
            }

            // Check if the tenant data is already cached
            if (tenantCache.TryGetValue(tenantName.ToLower(), out Tenant tenant))
            {
                // If tenant is found in cache, return the cached tenant context
                return Task.FromResult(new TenantContext<Tenant>(tenant));
            }

            // If tenant is not found in cache, query the database to resolve tenant details
            var company = _dbContext.CustomerDetails
                .Where(P => P.CompanyName.ToLower() == tenantName.ToLower())
                .FirstOrDefault();

            if (company != null)
            {
                tenant = new Tenant()
                {
                    Name = company.CompanyName.ToLower(),
                    Id = Convert.ToInt32(company.CompanyId),
                    ConnectionString = company.ConnectionStringOnline,
                    LogoUrl = company.LogoUrl, // Set the logo URL dynamically
					schemaname = company.schemaname
				};
            }
            else
            {
                tenant = null;
            }

            if (tenant != null)
            {
                // Add the tenant to the cache (Dictionary)
                tenantCache[tenantName.ToLower()] = tenant;

                // Return the tenant context
                return Task.FromResult(new TenantContext<Tenant>(tenant));
            }

            // Return null if tenant is not found
            return Task.FromResult<TenantContext<Tenant>>(null);
        }
    }
}
