using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using QD.ERP.Web.Service;
using System;
using System.Drawing;

namespace QD.ERP.Web.Areas.Finance.Reports.Payable_Statements
{
    public partial class EndDate : XtraReport
    {
        private SqlDataSource sqlDataSource1;
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public EndDate(
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

            // Initialize the SQL data source before configuration
            sqlDataSource1 = new SqlDataSource();

            // Optional: Add to report's components if needed
            this.DataSource = sqlDataSource1;

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

        public EndDate()
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
            if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                var connectionParams = new CustomStringConnectionParameters(tenant.ConnectionString);
                sqlDataSource1 = new SqlDataSource(connectionParams);

                var querySql = @"
            SELECT * 
            FROM qry205_027AgeingBillsReceivableWtColumns 
            WHERE (@AccountID IS NULL OR AccountHeadNo = @AccountID)
            AND VoucherDate BETWEEN @StartDate AND @EndDate";

                var customQuery = new CustomSqlQuery
                {
                    Name = "qry205_027AgeingBillsReceivableWtColumns",
                    Sql = querySql
                };

                customQuery.Parameters.AddRange(new[]
                {
            new QueryParameter("@AccountID", typeof(string), accountId ?? ""),
            new QueryParameter("@StartDate", typeof(DateTime), frmDate),
            new QueryParameter("@EndDate", typeof(DateTime), toDate)
        });

                sqlDataSource1.Queries.Clear();
                sqlDataSource1.Queries.Add(customQuery);
                sqlDataSource1.Name = "sqlDataSource1";

                sqlDataSource1.RebuildResultSchema();

                this.DataSource = sqlDataSource1;
                this.DataMember = "qry205_027AgeingBillsReceivableWtColumns";
            }
            else
            {
                throw new Exception("Unable to get tenant context. Please check session and cache.");
            }
        }

    }
}
