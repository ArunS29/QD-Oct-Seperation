using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SaasKit.Multitenancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QD.ERP.Web.Models.DALCommon; // Ensure this has ERPCommonContext
using QD.ERP.Web.DAL.Entities;     // Ensure this has ERPMasterWtDataContext
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Service
{
    public class TenantResolver : ITenantResolver<Tenant>
    {
        private readonly ERPCommonContext _dbContext;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "tenant_"; // Cache key prefix

        public TenantResolver(IMemoryCache cache, ERPCommonContext dbContext)
        {
            _dbContext = dbContext;
            _cache = cache;
        }

        public async Task<TenantContext<Tenant>> ResolveAsync(HttpContext context)
        {
            var tenantName = context.Items["TenantName"]?.ToString();

            if (string.IsNullOrEmpty(tenantName))
            {
                return null;
            }

            // Get or initialize tenant cache
            if (!_cache.TryGetValue(CacheKey, out Dictionary<string, Tenant> tenantCache))
            {
                tenantCache = new Dictionary<string, Tenant>();
                _cache.Set(CacheKey, tenantCache, TimeSpan.FromHours(1));
            }

            // Check cache first
            if (tenantCache.TryGetValue(tenantName.ToLower(), out Tenant cachedTenant))
            {
                return new TenantContext<Tenant>(cachedTenant);
            }

            // Fetch tenant metadata from ERPCommonContext
            var company = await _dbContext.CustomerDetails
                .FirstOrDefaultAsync(p => p.CompanyName.ToLower() == tenantName.ToLower());

            if (company == null)
                return null;

            var tenant = new Tenant
            {
                Name = company.CompanyName.ToLower(),
                Id = Convert.ToInt32(company.CompanyId),
                ConnectionString = company.ConnectionStringOnline,
                schemaname = company.schemaname
            };

            // Fetch logo from tenant-specific database
            var optionsBuilder = new DbContextOptionsBuilder<ERPMasterWtDataContext>();
            optionsBuilder.UseSqlServer(tenant.ConnectionString);

            using (var tenantDbContext = new ERPMasterWtDataContext(optionsBuilder.Options))
            {
                // 1. Find the default company ID from any row (null if not set)
                var defaultCompanyId = tenantDbContext.Tbl901CompanyDetails
                    .Select(c => c.DefaultcompanyID)
                    .FirstOrDefault(id => id.HasValue && id.Value != 0);

                // 2. Try to find the default row
                var companyInfo = tenantDbContext.Tbl901CompanyDetails
                    .AsEnumerable()
                    .FirstOrDefault(c => defaultCompanyId.HasValue && c.CompanyId == defaultCompanyId.Value);

                // 3. Fallback to first row if no default
                if (companyInfo == null)
                {
                    companyInfo = tenantDbContext.Tbl901CompanyDetails
                        .OrderBy(c => c.CompanyId)
                        .FirstOrDefault();
                }

                if (companyInfo != null)
                {
                    if (companyInfo.CompanyLogo != null)
                    {
                        tenant.LogoUrl = $"data:image/png;base64,{Convert.ToBase64String(companyInfo.CompanyLogo)}";
                    }

                    if (!string.IsNullOrEmpty(companyInfo.CompanyNameShort))
                    {
                        tenant.CompanyNameShort = companyInfo.CompanyNameShort.ToLower();
                    }

                    tenant.DefaultcompanyID = companyInfo.CompanyId.ToString();
                    tenant.DefaultcompanyName = companyInfo.CompanyName;
                    tenant.CompanyTextColor = companyInfo.CompanyTextColor.ToString();
                }
            }




        

            // Cache and return the resolved tenant
            tenantCache[tenantName.ToLower()] = tenant;
            return new TenantContext<Tenant>(tenant);
        }
    }
}
