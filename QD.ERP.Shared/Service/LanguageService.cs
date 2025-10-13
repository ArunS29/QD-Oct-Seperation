// File: QDERPWeb/Service/LanguageService.cs
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using QD.ERP.Shared.DAL.Entities;
//using HarfBuzzSharp;

namespace QD.ERP.Shared.Service
{
    public class LanguageService
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<LanguageService> _logger;

        public LanguageService(ILogger<LanguageService> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        public List<Language> GetLanguages()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    return dbContext.Languages.ToList();
                }
            }
            return new List<Language>();
        }
    }
}
