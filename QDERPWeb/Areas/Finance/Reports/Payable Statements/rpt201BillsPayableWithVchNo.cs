using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using System;

namespace QD.ERP.Web.Areas.Finance.Reports.Payable_Statements
{
    public partial class rpt201BillsPayableWithVchNo : XtraReport
    {
        private const string QueryName = "qry201SubLedgerPayablesMaster";

        public rpt201BillsPayableWithVchNo(string accountId, DateTime frmDate, DateTime toDate)
        {
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate);
        }

        public rpt201BillsPayableWithVchNo()
        {
            InitializeComponent();
            DateTime defaultDate = new DateTime(1753, 1, 1);
            SetReportParameters(null, defaultDate, defaultDate);
        }

        private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate)
        {
            // Add report parameters
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
                    Value = paramValue ?? DBNull.Value
                });
            }
            else
            {
                Parameters[paramName].Value = paramValue ?? DBNull.Value;
            }
        }

        private void AddSqlQueryParameters(string accountId, DateTime frmDate, DateTime toDate)
        {
            // Define the SQL query
            CustomSqlQuery selectQuery = new CustomSqlQuery()
            {
                Name = QueryName,
                Sql = @"SELECT * FROM qry201SubLedgerPayablesMaster
                        WHERE (@AccountID IS NULL OR AccountHeadNo = @AccountID)
                        AND VoucherDate BETWEEN @StartDate AND @EndDate"
            };

            // Add query parameters (use DBNull.Value for null values)
            selectQuery.Parameters.Add(new QueryParameter("@AccountID", typeof(string), accountId ?? (object)DBNull.Value));
            selectQuery.Parameters.Add(new QueryParameter("@StartDate", typeof(DateTime), frmDate.ToString("yyyy-MM-dd")));
            selectQuery.Parameters.Add(new QueryParameter("@EndDate", typeof(DateTime), toDate.ToString("yyyy-MM-dd")));

          
                // Attach query to SqlDataSource and fetch data
                sqlDataSource1.Queries.Clear();
                sqlDataSource1.Queries.Add(selectQuery);
                sqlDataSource1.Fill();

                CheckForEmptyData();
            }

        private void CheckForEmptyData()
        {
            var result = sqlDataSource1.Result["qry201SubLedgerPayablesMaster"];

            // Ensure the result is not null and check the row count via IList
            if (result == null || ((System.Collections.IList)result).Count == 0)
            {
                XRLabel noDataLabel = new XRLabel()
                {
                    Text = "No records found to display.",
                    BoundsF = new System.Drawing.RectangleF(0, 0, 650, 50),
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter,
                    Font = new System.Drawing.Font("Arial", 14, System.Drawing.FontStyle.Bold)
                };

                this.Bands[BandKind.Detail].Controls.Add(noDataLabel);
            }
        }
    }
}
