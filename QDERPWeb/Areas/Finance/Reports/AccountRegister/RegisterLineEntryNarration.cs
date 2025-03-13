using System;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;

namespace QD.ERP.Web.Areas.Finance.Reports.AccountRegister
{
    public partial class RegisterLineEntryNarration : DevExpress.XtraReports.UI.XtraReport
    {
        // Constructor with parameters to dynamically pass VoucherType, StartDate, EndDate
        public RegisterLineEntryNarration(string voucherType, DateTime frmDate, DateTime toDate)
        {
            InitializeComponent();
            SetReportParameters(voucherType, frmDate, toDate);

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
        public RegisterLineEntryNarration()
        {
            InitializeComponent();
        }

        // Method to set report and SQL query parameters
        private void SetReportParameters(string voucherType, DateTime frmDate, DateTime toDate)
        {
            // Ensure valid parameters
            voucherType ??= "DefaultType";
            frmDate = frmDate == DateTime.MinValue ? DateTime.Today : frmDate;
            toDate = toDate == DateTime.MinValue ? DateTime.Today : toDate;

            // Add or update report parameters
            AddReportParameter("VoucherType", typeof(string), voucherType);
            AddReportParameter("StartDate", typeof(DateTime), frmDate);
            AddReportParameter("EndDate", typeof(DateTime), toDate);

            // Set parameters for the SQL query
            ConfigureSqlDataSource(voucherType, frmDate, toDate);
        }

        // Helper method to add or update report parameters
        private void AddReportParameter(string paramName, Type paramType, object paramValue)
        {
            var parameter = Parameters[paramName];
            if (parameter == null)
            {
                Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter
                {
                    Name = paramName,
                    Type = paramType,
                    Value = paramValue,
                    Visible = false
                });
            }
            else
            {
                parameter.Value = paramValue;
                parameter.Visible = false;
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
