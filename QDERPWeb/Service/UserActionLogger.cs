using Microsoft.AspNetCore.Http;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Service;
using System;
using System.Threading.Tasks;

namespace QD.ERP.Web.Services.Logging
{
    public interface IUserActionLogger
    {
        Task LogAsync(string module, string actionDetail, string documentNo);
    }

    public class UserActionLogger : IUserActionLogger
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserActionLogger(TenantDbContextHelper tenantDbContextHelper, IHttpContextAccessor httpContextAccessor)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string module, string actionDetail, string documentNo)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var dbContext))
                return;

            string userName = _httpContextAccessor.HttpContext?.Session.GetString("UserName") ?? "Unknown";
            string userId = _httpContextAccessor.HttpContext?.Session.GetString("UserId") ?? "0";

            var log = new Tbl90116UserEntryLogSheet
            {
                EntryLogFor = module,
                EntryLogDetails = actionDetail,
                LogCreatedOn = DateTime.Now,
                LogCreatedBy = $"{userName} ({userId})",
                LogDocumentNo = documentNo
            };

            dbContext.Tbl90116UserEntryLogSheets.Add(log);
            await dbContext.SaveChangesAsync();
        }
    }
}
