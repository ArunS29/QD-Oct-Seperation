using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;

namespace QD.ERP.Web.Reports
{
    public partial class AccountExportLandscapeReport : DevExpress.XtraReports.UI.XtraReport
    {
        // Declare the query parameters and stored procedure query object
        private QueryParameter queryParameter1;
        private QueryParameter queryParameter2;
        private QueryParameter queryParameter3;
        private StoredProcQuery storedProcQuery1;

        // Constructor with parameters to dynamically pass AccountID, StartDate, EndDate
        public AccountExportLandscapeReport(string accountId, DateTime frmDate, DateTime toDate, string tenantName, string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb)
        {
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate, tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb);
        }

        // Parameterless constructor for design mode
        public AccountExportLandscapeReport()
        {
            InitializeComponent();
            SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "", null, "", "");
        }

        // Method to set the parameters dynamically
        private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate, string tenantName, string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb)
        {
            // Helper method to add or update a parameter
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

            // Add or update parameters
            AddOrUpdateParameter("AccountID", accountId ?? "", typeof(string));
            AddOrUpdateParameter("StartDate", frmDate == DateTime.MinValue ? DateTime.Today : frmDate, typeof(DateTime));
            AddOrUpdateParameter("EndDate", toDate == DateTime.MinValue ? DateTime.Today : toDate, typeof(DateTime));

            // New parameters for Tenant and Company Info
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyName", company_Name ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", company_address ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", Company_Name_Ar ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", company_address_arb ?? "", typeof(string), false);

            // Debug: Ensure logo URL is captured
            Console.WriteLine($"Company Logo: {logoImage != null}");


            // Bind TenantName and CompanyName to labels (update with actual control names)
            if (this.FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
            {
                tenantLabel.Text = tenantName;
            }
            if (this.FindControl("xrLabelCompanyAddress", true) is XRLabel companyNameLabel)
            {
                companyNameLabel.Text = company_Name;
            }

            if (this.FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
            {
                addressLabel.Text = company_address;
            }

            if (this.FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
            {
                logoPictureBox.Image = logoImage;
            }

            if (this.FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
            {
                companyNameArLabel.Text = Company_Name_Ar;
            }

            if (this.FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
            {
                addressArbLabel.Text = company_address_arb;
            }

            AddSqlQueryParameters( accountId,frmDate,toDate);

        }

        // Helper method to add or update report parameters
        private void AddReportParameter(string paramName, Type paramType, object paramValue)
        {
            if (Parameters[paramName] == null)
            {
                Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter()
                {
                    Name = paramName,
                    Type = paramType,
                    Value = paramValue,
                    Visible = false // Hide parameter panel
                });
            }
            else
            {
                Parameters[paramName].Value = paramValue;
                Parameters[paramName].Visible = false; // Ensure it's hidden
            }
        }

        // Method to dynamically set SQL query parameters
        private void AddSqlQueryParameters(string accountId, DateTime frmDate, DateTime toDate)
        {
            // Initialize the query parameters
            queryParameter1 = new QueryParameter();
            queryParameter2 = new QueryParameter();
            queryParameter3 = new QueryParameter();

            // Set the values for SQL query parameters
            queryParameter1.Name = "@ParamAccountNo";
            queryParameter1.Type = typeof(string);
            queryParameter1.ValueInfo = accountId ?? "L00567"; // Default value for AccountNo

            queryParameter2.Name = "@StartDate";
            queryParameter2.Type = typeof(DateTime);
            queryParameter2.ValueInfo = frmDate == DateTime.MinValue ? DateTime.Today.ToString("yyyy-MM-dd") : frmDate.ToString("yyyy-MM-dd");

            queryParameter3.Name = "@EndDate";
            queryParameter3.Type = typeof(DateTime);
            queryParameter3.ValueInfo = toDate == DateTime.MinValue ? DateTime.Today.ToString("yyyy-MM-dd") : toDate.ToString("yyyy-MM-dd");

            // Initialize the stored procedure query and set the StoredProcName
            storedProcQuery1 = new StoredProcQuery
            {
                Name = "StProAccountLedger", // Stored procedure name
                StoredProcName = "StProAccountLedger" // Replace with your actual stored procedure name
            };

            // Clear existing parameters and add the new ones
            storedProcQuery1.Parameters.Clear();
            storedProcQuery1.Parameters.AddRange(new QueryParameter[] {
                queryParameter1,
                queryParameter2,
                queryParameter3
            });

            // Re-assign the query to the SQL data source
            this.sqlDataSource1.Queries.Clear();
            this.sqlDataSource1.Queries.Add(storedProcQuery1);

            // Set the connection string for the data source
            this.sqlDataSource1.ConnectionName = "DBConnection"; // Your connection string name
            this.sqlDataSource1.Name = "sqlDataSource1";
        }
    }
}
