using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;
using System;

namespace QD.ERP.Web.Reports
{
    public partial class BillsReceivablelandscapeformat : XtraReport
    {
        // This is the constructor that will be used when the report is run with parameters.
        public BillsReceivablelandscapeformat(string accountId, DateTime frmDate, DateTime toDate)
        {
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate);
        }

        // Parameterless constructor for design mode (i.e., when opened in the report designer).
        // It uses default values so that it can be designed without real parameters.
        public BillsReceivablelandscapeformat()
        {
            InitializeComponent();

            DateTime defaultDate = DateTime.MinValue == DateTime.MinValue ? new DateTime(1753, 1, 1) : DateTime.MinValue;
            SetReportParameters(null, defaultDate, defaultDate);
        }

        private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate)
        {
            // Create report parameters
            AddReportParameter("AccountID", typeof(string), accountId);
            AddReportParameter("StartDate", typeof(DateTime), frmDate);
            AddReportParameter("EndDate", typeof(DateTime), toDate);

            // Set SQL query parameters
            AddSqlQueryParameters(accountId, frmDate, toDate);
        }

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

        private void AddSqlQueryParameters(string accountId, DateTime frmDate, DateTime toDate)
        {
            // Define the custom SQL query
            CustomSqlQuery selectQuery1 = new CustomSqlQuery()
            {
                Name = "qry201SubLedgerReceivablesMaster",
                Sql = @"SELECT * FROM qry201SubLedgerReceivablesMaster 
                        WHERE AccountHeadNo = @AccountID 
                        AND VoucherDate BETWEEN @StartDate AND @EndDate"
            };

            // Add parameters for filtering
            selectQuery1.Parameters.Add(new QueryParameter() { Name = "@AccountID", Type = typeof(string), ValueInfo = accountId });
            selectQuery1.Parameters.Add(new QueryParameter() { Name = "@StartDate", Type = typeof(DateTime), ValueInfo = frmDate.ToString("yyyy-MM-dd") });
            selectQuery1.Parameters.Add(new QueryParameter() { Name = "@EndDate", Type = typeof(DateTime), ValueInfo = toDate.ToString("yyyy-MM-dd") });

            // Add the query to the SqlDataSource
            this.sqlDataSource1.Queries.Clear();
            this.sqlDataSource1.Queries.Add(selectQuery1);
            this.sqlDataSource1.Fill();
        }
    }
}
