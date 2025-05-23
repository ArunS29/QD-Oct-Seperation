//using System;
//using System.Drawing;
//using DevExpress.DataAccess.ConnectionParameters;
//using DevExpress.DataAccess.Sql;
//using DevExpress.XtraReports.UI;
//using QD.ERP.Web.Service;

//namespace QD.ERP.Web.Areas.Finance.Reports
//{
//    public partial class BillsReceivableAgeingToday : XtraReport
//    {
//        private readonly TenantDbContextHelper _tenantDbContextHelper;

//        public BillsReceivableAgeingToday(
//            string accountId,
//            DateTime frmDate,
//            DateTime toDate,
//            string tenantName,
//            string companyName,
//            string companyAddress,
//            Image logoImage,
//            string companyNameAr,
//            string companyAddressArb, string username,
//            TenantDbContextHelper tenantDbContextHelper
//        )
//        {
//            _tenantDbContextHelper = tenantDbContextHelper;
//            InitializeComponent();
//            SetReportParameters(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb, username);
//        }

//        public BillsReceivableAgeingToday ()
//        {
//            InitializeComponent();
//            SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "", null, "", "","");
//        }

//        private void SetReportParameters(
//            string accountId,
//            DateTime frmDate,
//            DateTime toDate,
//            string tenantName,
//            string companyName,
//            string companyAddress,
//            Image logoImage,
//            string companyNameAr, string username,
//            string companyAddressArb)
//        {
//            void AddOrUpdateParameter(string name, object value, Type type, bool visible = false)
//            {
//                if (Parameters[name] == null)
//                {
//                    Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter()
//                    {
//                        Name = name,
//                        Type = type,
//                        Value = value,
//                        Visible = visible
//                    });
//                }
//                else
//                {
//                    Parameters[name].Value = value;
//                    Parameters[name].Visible = visible;
//                }
//            }

//            AddOrUpdateParameter("AccountID", accountId ?? "", typeof(string));
//            AddOrUpdateParameter("StartDate", frmDate == DateTime.MinValue ? DateTime.Today : frmDate, typeof(DateTime));
//            AddOrUpdateParameter("EndDate", toDate == DateTime.MinValue ? DateTime.Today : toDate, typeof(DateTime));
//            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
//            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
//            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
//            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
//            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string));
//            AddOrUpdateParameter("UserName", username ?? "", typeof(string));

//            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
//                tenantLabel.Text = tenantName;
//            if (this.FindControl("xrLabelUserName", true) is XRLabel userNameLabel)
//                userNameLabel.Text = username;
//            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
//                companyNameLabel.Text = companyName;

//            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
//                addressLabel.Text = companyAddress;

//            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox && logoImage != null)
//                logoPictureBox.Image = logoImage;

//            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
//                companyNameArLabel.Text = companyNameAr;

//            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
//                addressArbLabel.Text = companyAddressArb;

//            ConfigureDataSource(accountId, frmDate, toDate);
//        }

//        private void ConfigureDataSource(string accountId, DateTime frmDate, DateTime toDate)
//        {
//            if (_tenantDbContextHelper == null || !_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
//                throw new Exception("Unable to get tenant context. Please check session and cache.");

//            var connectionParams = new CustomStringConnectionParameters(tenant.ConnectionString);
//            sqlDataSource1 = new SqlDataSource(connectionParams);

//            var query = new CustomSqlQuery
//            {
//                Name = "qry201SubLedgerReceivablesMaster ",
//                Sql = @"SELECT * FROM qry201SubLedgerReceivablesMaster 
//                        WHERE (@AccountID IS NULL OR AccountHeadNo = @AccountID)
//                        AND VoucherDate BETWEEN @StartDate AND @EndDate"
//            };

//            query.Parameters.Add(new QueryParameter { Name = "@AccountID", Type = typeof(string), ValueInfo = accountId ?? "" });
//            query.Parameters.Add(new QueryParameter { Name = "@StartDate", Type = typeof(DateTime), ValueInfo = frmDate.ToString("yyyy-MM-dd") });
//            query.Parameters.Add(new QueryParameter { Name = "@EndDate", Type = typeof(DateTime), ValueInfo = toDate.ToString("yyyy-MM-dd") });

//            sqlDataSource1.Queries.Clear();
//            sqlDataSource1.Queries.Add(query);
//            sqlDataSource1.RebuildResultSchema(); // optional but recommended
//            sqlDataSource1.Fill();

//            this.DataSource = sqlDataSource1;
//            this.DataMember = "qry201SubLedgerReceivablesMaster ";
//        }
//    }
//}



using System;
using System.Drawing;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Areas.Finance.Reports
{
    public partial class BillsReceivableAgeingToday : XtraReport
    {
     
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public BillsReceivableAgeingToday(
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

            sqlDataSource1 = new SqlDataSource();
            this.DataSource = sqlDataSource1;

            SetReportParameters(accountId, frmDate,toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb, username);

            try
            {
                sqlDataSource1.Fill();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data: " + ex.Message, ex);
            }
        }

        public BillsReceivableAgeingToday()
        {
            InitializeComponent();
        }

        private void SetReportParameters(string accountId, DateTime frmDate,
            DateTime toDate, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressArb, string username)
        {
            var today = DateTime.Today;

            AddOrUpdateParameter("AccountID", accountId ?? "", typeof(string), false);
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

            ConfigureDataSource(accountId, frmDate,toDate);
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
            FROM qry20105BillsReceivableAgeingView 
            WHERE (@AccountID IS NULL OR AccountHeadNo = @AccountID)
            AND VoucherDate BETWEEN @StartDate AND @EndDate";

                var customQuery = new CustomSqlQuery
                {
                    Name = "qry20105BillsReceivableAgeingView",
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
                this.DataMember = "qry20105BillsReceivableAgeingView";
            }
            else
            {
                throw new Exception("Unable to get tenant context. Please check session and cache.");
            }
        }

    }
}
