using System;
using System.Drawing;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Reports
{
    public partial class AccountStatementFormat2Report : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public AccountStatementFormat2Report(
            string accountId,
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressArb,
            string username,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb, username);

            try
            {
                sqlDataSource2.Fill();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data: " + ex.Message, ex);
            }
        }

        public AccountStatementFormat2Report()
        {
            InitializeComponent();
        }

        private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressArb, string username)
        {
            accountId ??= "";
            frmDate = frmDate == DateTime.MinValue ? DateTime.Today : frmDate;
            toDate = toDate == DateTime.MinValue ? DateTime.Today : toDate;

            AddOrUpdateParameter("AccountID", accountId, typeof(string), false);
            AddOrUpdateParameter("StartDate", frmDate, typeof(DateTime), false);
            AddOrUpdateParameter("EndDate", toDate, typeof(DateTime), false);
            AddOrUpdateParameter("UserName", username ?? "", typeof(string), false);
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string), false);

            // Set control values
            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelUserName", true) is XRLabel userLabel)
                userLabel.Text = username;

            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = companyName;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = companyAddress;

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = companyNameAr;

            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = companyAddressArb;

            ConfigureSqlDataSource(accountId, frmDate, toDate);
        }

        private void AddOrUpdateParameter(string paramName, object paramValue, Type paramType, bool visible)
        {
            var parameter = Parameters[paramName];
            if (parameter == null)
            {
                Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter
                {
                    Name = paramName,
                    Type = paramType,
                    Value = paramValue,
                    Visible = visible
                });
            }
            else
            {
                parameter.Value = paramValue;
                parameter.Visible = visible;
            }
        }

        private void ConfigureSqlDataSource(string accountId, DateTime frmDate, DateTime toDate)
        {
            sqlDataSource2.Queries.Clear();

            if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                sqlDataSource2.ConnectionParameters = new CustomStringConnectionParameters(tenant.ConnectionString);

                string schemaName = string.IsNullOrWhiteSpace(tenant.schemaname) ? "dbo" : tenant.schemaname;
                string fullStoredProcName = $"{schemaName}.StProAccountLedger";

                var storedProcQuery = new StoredProcQuery
                {
                    Name = "StProAccountLedger",
                    StoredProcName = fullStoredProcName
                };

                storedProcQuery.Parameters.AddRange(new[]
                {
            new QueryParameter("@ParamAccountNo", typeof(string), accountId),
            new QueryParameter("@StartDate", typeof(DateTime), frmDate),
            new QueryParameter("@EndDate", typeof(DateTime), toDate)
        });

                sqlDataSource2.Queries.Add(storedProcQuery);
                sqlDataSource2.Name = "sqlDataSource2";
            }
            else
            {
                throw new Exception("Unable to get tenant context. Please check session and cache.");
            }
        }
    }
}
