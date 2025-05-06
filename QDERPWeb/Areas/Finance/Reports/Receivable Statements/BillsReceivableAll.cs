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
            string companyAddressArb, string username,
            TenantDbContextHelper tenantDbContextHelper
        )
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb, username);
        }

        public BillsReceivableAll()
        {
            InitializeComponent();
            SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "", null, "", "","");
        }

        private void SetReportParameters(
            string accountId,
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr, string username,
            string companyAddressArb)
        {
            void AddOrUpdateParameter(string name, object value, Type type, bool visible = false)
            {
                if (Parameters[name] == null)
                {
                    Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter()
                    {
                        Name = name,
                        Type = type,
                        Value = value,
                        Visible = visible
                    });
                }
                else
                {
                    Parameters[name].Value = value;
                    Parameters[name].Visible = visible;
                }
            }

            AddOrUpdateParameter("AccountID", accountId ?? "", typeof(string));
            AddOrUpdateParameter("StartDate", frmDate == DateTime.MinValue ? DateTime.Today : frmDate, typeof(DateTime));
            AddOrUpdateParameter("EndDate", toDate == DateTime.MinValue ? DateTime.Today : toDate, typeof(DateTime));
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string));
            AddOrUpdateParameter("UserName", username ?? "", typeof(string));

            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;
            if (this.FindControl("xrLabelUserName", true) is XRLabel userNameLabel)
                userNameLabel.Text = username;
            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = companyName;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = companyAddress;

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox && logoImage != null)
                logoPictureBox.Image = logoImage;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = companyNameAr;

            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = companyAddressArb;

            ConfigureDataSource(accountId, frmDate, toDate);
        }

        private void ConfigureDataSource(string accountId, DateTime frmDate, DateTime toDate)
        {
            if (_tenantDbContextHelper == null || !_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                throw new Exception("Unable to get tenant context. Please check session and cache.");

            var connectionParams = new CustomStringConnectionParameters(tenant.ConnectionString);
            sqlDataSource1 = new SqlDataSource(connectionParams);

            var query = new CustomSqlQuery
            {
                Name = "qry201SubLedgerReceivablesMaster ",
                Sql = @"SELECT * FROM qry201SubLedgerReceivablesMaster 
                        WHERE (@AccountID IS NULL OR AccountHeadNo = @AccountID)
                        AND VoucherDate BETWEEN @StartDate AND @EndDate"
            };

            query.Parameters.Add(new QueryParameter { Name = "@AccountID", Type = typeof(string), ValueInfo = accountId ?? "" });
            query.Parameters.Add(new QueryParameter { Name = "@StartDate", Type = typeof(DateTime), ValueInfo = frmDate.ToString("yyyy-MM-dd") });
            query.Parameters.Add(new QueryParameter { Name = "@EndDate", Type = typeof(DateTime), ValueInfo = toDate.ToString("yyyy-MM-dd") });

            sqlDataSource1.Queries.Clear();
            sqlDataSource1.Queries.Add(query);
            sqlDataSource1.RebuildResultSchema(); // optional but recommended
            sqlDataSource1.Fill();

            this.DataSource = sqlDataSource1;
            this.DataMember = "qry201SubLedgerReceivablesMaster ";
        }
    }
}
