using System.Drawing;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using System;
using System.Drawing;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;
using QD.ERP.Web.Service; // Needed for TenantDbContextHelper


namespace QD.ERP.Web.Areas.Finance.Reports
{
    public partial class BillsReceivableAgeingToday : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        public BillsReceivableAgeingToday(string accountId, DateTime frmDate, DateTime toDate, string tenantName, string company_Name, string company_address, Image logoImage, string companyNameAr, string companyAddressArb, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate, tenantName, company_Name, company_address, logoImage, companyNameAr, companyAddressArb);
        }

        public BillsReceivableAgeingToday()
        {
            InitializeComponent();
            SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "", null, "", "");
        }

        private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate, string tenantName, string company_Name, string company_address, Image logoImage, string companyNameAr, string companyAddressArb)
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

            // Add report parameters
            AddOrUpdateParameter("AccountID", accountId ?? "", typeof(string));
            AddOrUpdateParameter("StartDate", frmDate == DateTime.MinValue ? DateTime.Today : frmDate, typeof(DateTime));
            AddOrUpdateParameter("EndDate", toDate == DateTime.MinValue ? DateTime.Today : toDate, typeof(DateTime));
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", company_Name ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", company_address ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string));

            // Bind parameters to UI controls
            if (this.FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (this.FindControl("xrLabelCompanyAddress", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = company_Name;

            if (this.FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = company_address;

            if (this.FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;

            if (this.FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = companyNameAr;

            if (this.FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = companyAddressArb;

            // Set up SQL query
            AddSqlQueryParameters(accountId, frmDate, toDate);
        }

        private void AddSqlQueryParameters(string accountId, DateTime frmDate, DateTime toDate)
        {
            CustomSqlQuery selectQuery = new CustomSqlQuery()
            {
                Name = "qry20105BillsReceivableAgeingView",
                Sql = @"SELECT * FROM qry20105BillsReceivableAgeingView
                        WHERE (@AccountID IS NULL OR AccountHeadNo = @AccountID) 
                        AND VoucherDate BETWEEN @StartDate AND @EndDate"
            };

            selectQuery.Parameters.Add(new QueryParameter() { Name = "@AccountID", Type = typeof(string), ValueInfo = accountId });
            selectQuery.Parameters.Add(new QueryParameter() { Name = "@StartDate", Type = typeof(DateTime), ValueInfo = frmDate.ToString("yyyy-MM-dd") });
            selectQuery.Parameters.Add(new QueryParameter() { Name = "@EndDate", Type = typeof(DateTime), ValueInfo = toDate.ToString("yyyy-MM-dd") });

            this.sqlDataSource1.Queries.Clear();
            this.sqlDataSource1.Queries.Add(selectQuery);
            this.sqlDataSource1.Fill();

            // Connection string logic
            if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                var connectionString = tenant.ConnectionString;
                var connectionParams = new CustomStringConnectionParameters(connectionString);
                sqlDataSource1.ConnectionParameters = connectionParams;
            }
            else
            {
                throw new Exception("Unable to get tenant context. Please check session and cache.");
            }
        }


    }
}
