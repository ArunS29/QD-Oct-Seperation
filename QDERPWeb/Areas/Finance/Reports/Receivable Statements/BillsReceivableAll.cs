using System;
using System.Drawing;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;

namespace QD.ERP.Web.Areas.Finance.Reports
{
    public partial class BillsReceivableAll : XtraReport
    {
        public BillsReceivableAll(string accountId, DateTime frmDate, DateTime toDate, string tenantName, string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb)
        {
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate, tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb);
        }

        public BillsReceivableAll()
        {
            InitializeComponent();
            SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "", null, "", "");
        }

        private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate, string tenantName, string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb)
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
            //AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            //AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string));

            // Bind parameters to UI controls
            if (this.FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (this.FindControl("xrLabelCompanyAddress", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = company_Name;

            if (this.FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = company_address;

            if (this.FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;

            //if (this.FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
            //    companyNameArLabel.Text = companyNameAr;

            //if (this.FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
            //    addressArbLabel.Text = companyAddressArb;

            // Set up SQL query
            AddSqlQueryParameters(accountId, frmDate, toDate);
        }

        private void AddSqlQueryParameters(string accountId, DateTime frmDate, DateTime toDate)
        {
            CustomSqlQuery selectQuery = new CustomSqlQuery()
            {
                Name = "qry201SubLedgerReceivablesMaster",
                Sql = @"SELECT * FROM qry201SubLedgerReceivablesMaster 
                        WHERE (@AccountID IS NULL OR AccountHeadNo = @AccountID) 
                        AND VoucherDate BETWEEN @StartDate AND @EndDate"
            };

            selectQuery.Parameters.Add(new QueryParameter() { Name = "@AccountID", Type = typeof(string), ValueInfo = accountId });
            selectQuery.Parameters.Add(new QueryParameter() { Name = "@StartDate", Type = typeof(DateTime), ValueInfo = frmDate.ToString("yyyy-MM-dd") });
            selectQuery.Parameters.Add(new QueryParameter() { Name = "@EndDate", Type = typeof(DateTime), ValueInfo = toDate.ToString("yyyy-MM-dd") });

            sqlDataSource1.Queries.Clear();
            sqlDataSource1.Queries.Add(selectQuery);
            sqlDataSource1.Fill();
        }
    }
}
