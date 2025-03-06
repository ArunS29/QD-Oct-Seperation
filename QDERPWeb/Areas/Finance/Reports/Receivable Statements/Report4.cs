using System;

using DevExpress.DataAccess.Sql;

using DevExpress.XtraReports.UI;

namespace QD.ERP.Web.Reports

{

    public partial class Report4 : DevExpress.XtraReports.UI.XtraReport

    {

        public Report4(string accountId, DateTime frmDate, DateTime toDate)

        {

            InitializeComponent();

            SetReportParameters(accountId, frmDate, toDate);

        }

        public Report4()

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

            // Validate Data Source

            ValidateQueryResult();

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

            selectQuery.Parameters.Add(new QueryParameter() { Name = "@AccountID", Type = typeof(string), ValueInfo = accountId });

            selectQuery.Parameters.Add(new QueryParameter() { Name = "@StartDate", Type = typeof(DateTime), ValueInfo = frmDate.ToString("yyyy-MM-dd") });

            selectQuery.Parameters.Add(new QueryParameter() { Name = "@EndDate", Type = typeof(DateTime), ValueInfo = toDate.ToString("yyyy-MM-dd") });

            // Attach query to SqlDataSource

            this.sqlDataSource1.Queries.Clear();

            this.sqlDataSource1.Queries.Add(selectQuery);

            this.sqlDataSource1.Fill();

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
