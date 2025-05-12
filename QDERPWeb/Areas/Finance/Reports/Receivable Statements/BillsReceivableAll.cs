using System;
using System.Drawing;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.Finance.Reports
{
    public partial class BillsReceivableAll : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public BillsReceivableAll(
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
                sqlDataSource1.Fill();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data: " + ex.Message, ex);
            }
        }

        public BillsReceivableAll()
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
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string), false);
            AddOrUpdateParameter("UserName", username ?? "", typeof(string), false);

            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelUserName", true) is XRLabel userNameLabel)
                userNameLabel.Text = username;

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

            ConfigureDataSource(accountId, frmDate, toDate);
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

        private void ConfigureDataSource(string accountId, DateTime frmDate, DateTime toDate)
        {
            sqlDataSource1.Queries.Clear();

            if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                sqlDataSource1.ConnectionParameters = new CustomStringConnectionParameters(tenant.ConnectionString);

                string querySql = @"SELECT * FROM qry201SubLedgerReceivablesMaster 
                                WHERE (@AccountID IS NULL OR AccountHeadNo = @AccountID)
                                AND VoucherDate BETWEEN @StartDate AND @EndDate";

                var customQuery = new CustomSqlQuery
                {
                    Name = "qry201SubLedgerReceivablesMaster",
                    Sql = querySql
                };

                customQuery.Parameters.AddRange(new[]
                {
                new QueryParameter("@AccountID", typeof(string), accountId),
                new QueryParameter("@StartDate", typeof(DateTime), frmDate),
                new QueryParameter("@EndDate", typeof(DateTime), toDate)
            });

                sqlDataSource1.Queries.Add(customQuery);
                sqlDataSource1.Name = "sqlDataSource1";
            }
            else
            {
                throw new Exception("Unable to get tenant context. Please check session and cache.");
            }
        }
    }
}
