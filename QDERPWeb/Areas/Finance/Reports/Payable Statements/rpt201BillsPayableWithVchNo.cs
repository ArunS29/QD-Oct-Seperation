using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using QD.ERP.Web.Service;
using System;
using System.Drawing;

namespace QD.ERP.Web.Areas.Finance.Reports.Payable_Statements
{
    public partial class rpt201BillsPayableWithVchNo : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public rpt201BillsPayableWithVchNo(
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
            SetReportParameters(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb,username);
            ConfigureDataSource(accountId);
        }

        public rpt201BillsPayableWithVchNo()
        {
            InitializeComponent();
        }

        private void ConfigureDataSource(string accountId)
        {
            sqlDataSource1.Queries.Clear();
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                throw new Exception("Unable to get tenant context. Please check session and cache.");

            var connectionParams = new CustomStringConnectionParameters(tenant.ConnectionString);
            sqlDataSource1 = new SqlDataSource(connectionParams);

            var query = new CustomSqlQuery
            {
                Name = "qry201SubLedgerPayablesMaster ",
                Sql = "SELECT * FROM qry201SubLedgerPayablesMaster  WHERE AccountHeadNo = @AccountID"
            };

            query.Parameters.Add(new QueryParameter
            {
                Name = "AccountID",
                Type = typeof(string),
                Value = accountId ?? ""
            });

            sqlDataSource1.Queries.Clear();
            sqlDataSource1.Queries.Add(query);

            sqlDataSource1.RebuildResultSchema();
            sqlDataSource1.Fill();

            this.DataSource = sqlDataSource1;
            this.DataMember = "qry201SubLedgerPayablesMaster ";
        }

        private void SetReportParameters(
            string accountId,
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressArb,
            string username)
        {
            void AddOrUpdateParameter(string paramName, object paramValue, Type paramType, bool visible)
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

            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = companyName;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = companyAddress;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = companyNameAr;

            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = companyAddressArb;
            if (FindControl("xrLabelUserName", true) is XRLabel userLabel)
                userLabel.Text = username;

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox && logoImage != null)
                logoPictureBox.Image = logoImage;
        }
    }
}
