using System;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;
using System.Drawing;

namespace QD.ERP.Web.Areas.Finance.Reports.AccountRegister
{
    public partial class RegisterWithVchNarration : DevExpress.XtraReports.UI.XtraReport
    {
        // Constructor with parameters to dynamically pass VoucherType, StartDate, EndDate, and company details
        public RegisterWithVchNarration(string voucherType, DateTime frmDate, DateTime toDate, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressArb)
        {
            InitializeComponent();
            SetReportParameters(voucherType, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb);

            try
            {
                this.sqlDataSource1.Fill(); // Ensure data is fetched immediately
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data: " + ex.Message, ex);
            }
        }

        // Parameterless constructor for design mode
        public RegisterWithVchNarration()
        {
            InitializeComponent();
        }

        // Method to set report and SQL query parameters
        private void SetReportParameters(string voucherType, DateTime frmDate, DateTime toDate, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressArb)
        {
            // Ensure valid parameters
            voucherType ??= "DefaultType";
            frmDate = frmDate == DateTime.MinValue ? DateTime.Today : frmDate;
            toDate = toDate == DateTime.MinValue ? DateTime.Today : toDate;

            // Add or update report parameters
            AddOrUpdateParameter("VoucherType", voucherType, typeof(string), false);
            AddOrUpdateParameter("StartDate", frmDate, typeof(DateTime), false);
            AddOrUpdateParameter("EndDate", toDate, typeof(DateTime), false);

            // Add company details parameters
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string), false);

            // Debug: Ensure logo is captured
            Console.WriteLine($"Company Logo: {logoImage != null}");

            // Bind values to report controls
            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

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

            // Set parameters for the SQL query
            ConfigureSqlDataSource(voucherType, frmDate, toDate);
        }

        // Helper method to add or update report parameters
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

        // Configure the SQL Data Source and add parameters
        private void ConfigureSqlDataSource(string voucherType, DateTime frmDate, DateTime toDate)
        {
            // Ensure clean query setup
            sqlDataSource1.Queries.Clear();

            // Define the stored procedure query
            var storedProcQuery = new StoredProcQuery
            {
                Name = "StProAccountLedgerByVoucherType",
                StoredProcName = "StProAccountLedgerByVoucherType"
            };

            // Add parameters to stored procedure
            storedProcQuery.Parameters.AddRange(new[]
            {
                new QueryParameter("@VoucherType", typeof(string), voucherType),
                new QueryParameter("@StartDate", typeof(DateTime), frmDate),
                new QueryParameter("@EndDate", typeof(DateTime), toDate)
            });

            // Reassign the query to the SQL data source
            sqlDataSource1.Queries.Add(storedProcQuery);

            // Ensure the correct connection string name
            sqlDataSource1.ConnectionName = "DBConnection";

            // Attempt to fill the data source
            try
            {
                sqlDataSource1.Fill();
            }
            catch (Exception ex)
            {
                throw new Exception("Error filling data source: " + ex.Message, ex);
            }
        }
    }
}
