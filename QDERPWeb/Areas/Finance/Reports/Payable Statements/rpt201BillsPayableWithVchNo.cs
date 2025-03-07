using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;

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
            if (!DesignMode)
            {
                AddSqlQueryParameters(accountId, frmDate, toDate);
            }
        }

        private void AddReportParameter(string paramName, Type paramType, object paramValue)
        {
            if (Parameters[paramName] == null)
            {
                Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter()
                {
                    Name = paramName,
                    Type = paramType,
                    Value = paramValue ?? DBNull.Value,
                      Visible = false
                });
            }
            else
            {
                Parameters[paramName].Value = paramValue ?? DBNull.Value;
                Parameters[paramName].Visible = false;
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
            selectQuery.Parameters.Add(new QueryParameter("@StartDate", typeof(string), frmDate.ToString("yyyy-MM-dd")));
            selectQuery.Parameters.Add(new QueryParameter("@EndDate", typeof(string), toDate.ToString("yyyy-MM-dd")));

            // Attach query to SqlDataSource and fetch data
            sqlDataSource1.Queries.Clear();
            sqlDataSource1.Queries.Add(selectQuery);
            sqlDataSource1.Fill();

            CheckForEmptyData();
        }

        private void CheckForEmptyData()
        {
            if (sqlDataSource1.Result[QueryName] is IList result && result.Count == 0)
            {
                XRLabel noDataLabel = new XRLabel()
                {
                    Text = "No records found to display.",
                    BoundsF = new RectangleF(0, 0, 650, 50),
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter,
                    Font = new Font("Arial", 14, FontStyle.Bold)
                };

                this.Bands[BandKind.Detail].Controls.Add(noDataLabel);
            }
        }


    }
}