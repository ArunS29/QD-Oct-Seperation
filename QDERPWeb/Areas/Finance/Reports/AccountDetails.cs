using System;
using System.Drawing;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;

namespace QD.ERP.Web.Areas.Finance.Reports
{
    public partial class AccountDetails : XtraReport
    {
        private QueryParameter queryParameter1;
        private QueryParameter queryParameter2;
        private QueryParameter queryParameter3;
        private StoredProcQuery storedProcQuery1;

        // Constructor with parameters to dynamically pass AccountID, StartDate, EndDate
        public AccountDetails(string accountId, DateTime frmDate, DateTime toDate)
        {
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate);
        }

        // Parameterless constructor for design mode
        public AccountDetails()
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
            queryParameter1 = new QueryParameter("@ParamAccountNo", typeof(string), accountId ?? "L00567");

            queryParameter2 = new QueryParameter("@StartDate", typeof(DateTime),
                frmDate == DateTime.MinValue ? DateTime.Today.ToString("yyyy-MM-dd") : frmDate.ToString("yyyy-MM-dd"));

            queryParameter3 = new QueryParameter("@EndDate", typeof(DateTime),
                toDate == DateTime.MinValue ? DateTime.Today.ToString("yyyy-MM-dd") : toDate.ToString("yyyy-MM-dd"));

            // Initialize the stored procedure query and set the StoredProcName
            storedProcQuery1 = new StoredProcQuery
            {
                Name = "StProAccountLedger",
                StoredProcName = "StProAccountLedger" // Replace with your actual stored procedure name
            };

            // Clear existing parameters and add the new ones
            storedProcQuery1.Parameters.Clear();
            storedProcQuery1.Parameters.AddRange(new[] { queryParameter1, queryParameter2, queryParameter3 });

            // Re-assign the query to the SQL data source
            this.sqlDataSource1.Queries.Clear();
            this.sqlDataSource1.Queries.Add(storedProcQuery1);

            // Set the connection string for the data source
            this.sqlDataSource1.ConnectionName = "DBConnection"; // Your connection string name
            this.sqlDataSource1.Name = "sqlDataSource1";
        }
    }
}
