using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;

namespace QD.ERP.Web.Areas.Finance.Reports
{
    public partial class BillsReceivableAll : XtraReport
    {
        public BillsReceivableAll(string accountId, DateTime frmDate, DateTime toDate)
        {
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate);
        }

        public BillsReceivableAll()
        {
            InitializeComponent();

            DateTime defaultDate = DateTime.MinValue == DateTime.MinValue ? new DateTime(1753, 1, 1) : DateTime.MinValue;
            SetReportParameters(null, defaultDate, defaultDate);
        }

        private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate)
        {
            // Create and set report parameters
            AddReportParameter("AccountID", typeof(string), accountId);
            AddReportParameter("StartDate", typeof(DateTime), frmDate);
            AddReportParameter("EndDate", typeof(DateTime), toDate);

            // Set up SQL query
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

        private void AddSqlQueryParameters(string accountId, DateTime frmDate, DateTime toDate)
        {
            // Define the SQL query
            CustomSqlQuery selectQuery = new CustomSqlQuery()
            {
                Name = "qry201SubLedgerReceivablesMaster",
                Sql = @"SELECT * FROM qry201SubLedgerReceivablesMaster 
                        WHERE (@AccountID IS NULL OR AccountHeadNo = @AccountID) 
                        AND VoucherDate BETWEEN @StartDate AND @EndDate"
            };

            // Add query parameters
            selectQuery.Parameters.Add(new QueryParameter() { Name = "@AccountID", Type = typeof(string), ValueInfo = accountId });
            selectQuery.Parameters.Add(new QueryParameter() { Name = "@StartDate", Type = typeof(DateTime), ValueInfo = frmDate.ToString("yyyy-MM-dd") });
            selectQuery.Parameters.Add(new QueryParameter() { Name = "@EndDate", Type = typeof(DateTime), ValueInfo = toDate.ToString("yyyy-MM-dd") });

            // Attach query to SqlDataSource
            this.sqlDataSource1.Queries.Clear();
            this.sqlDataSource1.Queries.Add(selectQuery);
            this.sqlDataSource1.Fill();
        }
    }
}

