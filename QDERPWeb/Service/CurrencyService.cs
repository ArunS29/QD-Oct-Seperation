using Azure.Core;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
//using QD.ERP.Web.Models.DALCommon;
using QD.ERP.Web.Models.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Azure.Communication.Email;
using QD.ERP.Web.Areas.Utility;
using System.Net.Mail;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;

namespace QD.ERP.Web.Service
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
