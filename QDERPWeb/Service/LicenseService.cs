using Microsoft.EntityFrameworkCore;
using QD.ERP.Web.DAL.Entities;
using QD.ERP.Web.Models.DALCommon;
using System;
using System.Threading.Tasks;

namespace QD.ERP.Web.Service
{
    public class LicenseService
    {
        private readonly ERPCommonContext _dbContext;

        public LicenseService(ERPCommonContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CustomerDetail> GetLicenseInfoAsync(string companyName)
        {
            var licenseInfo = await _dbContext.CustomerDetails
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CompanyName.ToLower() == companyName.ToLower());

            return licenseInfo;
        }

        public async Task<bool> IsLicenseValidAsync(string companyName)
        {
            var licenseInfo = await GetLicenseInfoAsync(companyName);
            if (licenseInfo == null) return false;

            var expiryDate = licenseInfo.LicExpiryDate ?? DateTime.MinValue;
            var extendedDays = licenseInfo.ExtendedDays ?? 0;
            var validUntil = expiryDate.AddDays(Convert.ToDouble(extendedDays));


            return DateTime.UtcNow.Date <= validUntil.Date;
        }
    }
}
