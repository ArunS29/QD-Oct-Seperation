using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.DataFederation;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using QD.ERP.Web.Service;
using System;
using System.Drawing;
using System.Data;

namespace QD.ERP.Web.Areas.Finance.Reports.Payable_Statements
{
    public partial class Balance : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public Balance(
            string accountId,
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressArb,
            TenantDbContextHelper tenantDbContextHelper
        )
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb);
            ConfigureDataSource(accountId);
        }

        public Balance()
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
                Name = "qry205_017AgeingBillsPayableWtColumns", // Change to match your report
                Sql = "SELECT * FROM qry205_017AgeingBillsPayableWtColumns WHERE AccountHeadNo = @AccountID"
            };

            query.Parameters.Add(new QueryParameter
            {
                Name = "AccountID",
                Type = typeof(string),
                Value = accountId ?? ""
            });

            sqlDataSource1.Queries.Clear();
            sqlDataSource1.Queries.Add(query);

            sqlDataSource1.RebuildResultSchema(); // Optional
            sqlDataSource1.Fill(); // Important

            this.DataSource = sqlDataSource1;
            this.DataMember = "qry205_017AgeingBillsPayableWtColumns"; // Match query name
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
            string companyAddressArb)
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

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox && logoImage != null)
                logoPictureBox.Image = logoImage;
        }
        private void CheckForEmptyData()
        {
            if (sqlDataSource1?.Result != null && !string.IsNullOrEmpty(DataMember))
            {
                var resultTable = sqlDataSource1.Result[DataMember] as System.Data.DataTable;
                if (resultTable == null || resultTable.Rows.Count == 0)
                {
                    XRLabel noDataLabel = new XRLabel()
                    {
                        Text = "No records found to display.",
                        BoundsF = new RectangleF(0, 0, 850, 50),
                        TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter,
                        Font = new Font("Arial", 14, FontStyle.Bold),
                        ForeColor = Color.Red
                    };

                    this.Bands[BandKind.Detail].Controls.Clear();
                    this.Bands[BandKind.Detail].Controls.Add(noDataLabel);
                }
            }
        }

    }
}
