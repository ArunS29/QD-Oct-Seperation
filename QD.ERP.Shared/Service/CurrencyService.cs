using Azure.Core;
using QD.ERP.Shared.DAL.Entities;
using QD.ERP.Shared.Service;
//using QD.ERP.Shared.Models.DALCommon;
using QD.ERP.Shared.Models.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Azure.Communication.Email;
using QD.ERP.Shared.Areas.Utility;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace QD.ERP.Shared.Service
{
    public class CurrencyService
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly ILogger<CurrencyService> _logger;

        public CurrencyService(ILogger<CurrencyService> logger, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _logger = logger;
        }

        public List<CurrencyMaster> GetCurrencies()
        {
            if (_tenantDbContextHelper.TryGetTenantAndDbContext(out Tenant tenant, out ERPMasterWtDataContext dbContext))
            {
                using (dbContext)
                {
                    return dbContext.CurrencyMasters.ToList();
                }
            }
            return new List<CurrencyMaster>();
        }
    }
}
