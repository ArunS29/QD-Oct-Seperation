using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using System;

namespace QD.ERP.Web.Areas.Finance.Reports.Receivable_Statements
{
    public partial class BillsReceivableLedgerBalance : XtraReport
    {
        public BillsReceivableLedgerBalance(string accountId, DateTime frmDate, DateTime toDate)
        {
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate);
        }

        public BillsReceivableLedgerBalance()
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
            // Define the SQL query
            CustomSqlQuery selectQuery = new CustomSqlQuery()
            {
                Name = "qry205_027AgeingBillsReceivableWtColumns",
                Sql = @"SELECT * FROM qry205_027AgeingBillsReceivableWtColumns
                        WHERE (@AccountID IS NULL OR AccountHeadNo = @AccountID)
                        AND VoucherDate BETWEEN @StartDate AND @EndDate"
            };

            // Add query parameters
            selectQuery.Parameters.Add(new QueryParameter("@AccountID", typeof(string), accountId));
            selectQuery.Parameters.Add(new QueryParameter("@StartDate", typeof(DateTime), frmDate.ToString("yyyy-MM-dd")));
            selectQuery.Parameters.Add(new QueryParameter("@EndDate", typeof(DateTime), toDate.ToString("yyyy-MM-dd")));

            // Attach query to SqlDataSource
            sqlDataSource1.Queries.Clear();
            sqlDataSource1.Queries.Add(selectQuery);
            sqlDataSource1.Fill();

            // Validate Data Source
            ValidateQueryResult();
        }

        private void ValidateQueryResult()
        {
            var result = sqlDataSource1.Result["qry205_027AgeingBillsReceivableWtColumns"];

            // Ensure the result is not null and check the row count via IList
            if (result == null || ((System.Collections.IList)result).Count == 0)
            {
                throw new InvalidOperationException("No data returned from the SQL query. Please check the query and parameters.");
            }
        }


    }
}
