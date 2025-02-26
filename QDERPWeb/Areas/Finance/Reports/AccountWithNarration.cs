using System;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;
using Humanizer.Localisation;

namespace QD.ERP.Web.Reports
{
    public partial class AccountWithNarration : DevExpress.XtraReports.UI.XtraReport
    {
        // Declare the query parameters and stored procedure query object
        private QueryParameter queryParameter1;
        private QueryParameter queryParameter2;
        private QueryParameter queryParameter3;
        private StoredProcQuery storedProcQuery1;

        // Constructor with parameters to dynamically pass AccountID, StartDate, EndDate
        public AccountWithNarration(string accountId, DateTime frmDate, DateTime toDate)
        {
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate);
        }

        // Parameterless constructor for design mode
        public AccountWithNarration()
        {
            InitializeComponent();
            SetReportParameters(null, DateTime.MinValue, DateTime.MinValue);
        }

        // Method to set the parameters dynamically
        private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate)
        {
            // Add report parameters
            AddReportParameter("AccountID", typeof(string), accountId ?? "");
            AddReportParameter("StartDate", typeof(DateTime), frmDate == DateTime.MinValue ? DateTime.Today : frmDate);
            AddReportParameter("EndDate", typeof(DateTime), toDate == DateTime.MinValue ? DateTime.Today : toDate);

            // Set parameters for the SQL query
            AddSqlQueryParameters(accountId, frmDate, toDate);
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
                    Value = paramValue
                });
            }
            else
            {
                Parameters[paramName].Value = paramValue;
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
            queryParameter2.ValueInfo = frmDate == DateTime.MinValue ? "2020-01-01" : frmDate.ToString("yyyy-MM-dd");

            queryParameter3.Name = "@EndDate";
            queryParameter3.Type = typeof(DateTime);
            queryParameter3.ValueInfo = toDate == DateTime.MinValue ? "2021-12-01" : toDate.ToString("yyyy-MM-dd");

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

            // Set the result schema (optional)
        }
    }
}
