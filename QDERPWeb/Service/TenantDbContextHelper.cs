using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Collections.Generic;

public class TenantDbContextHelper
{
    private readonly IMemoryCache _cache;
    private readonly DbContextFactory _dbContextFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TenantDbContextHelper> _logger;

    public TenantDbContextHelper(IMemoryCache cache, DbContextFactory dbContextFactory, IHttpContextAccessor httpContextAccessor, ILogger<TenantDbContextHelper> logger)
    {
        _cache = cache;
        _dbContextFactory = dbContextFactory;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public bool TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext)
    {
        tenant = null;
        dbContext = null;

        var tenantName = _httpContextAccessor.HttpContext.Session.GetString("TenantName");
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
                _logger.LogError($"Error creating DbContext: {ex.Message}");
            }
        }

        return false;
    }
}
