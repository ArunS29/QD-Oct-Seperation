using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;
using System;

namespace QD.ERP.Web.Areas.Finance.Reports.Payable_Statements
{
    public partial class EndDate : XtraReport
    {
        private SqlDataSource sqlDataSource1;

        public EndDate(string accountId, DateTime frmDate, DateTime toDate)
        {
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate);
        }

        public EndDate()
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
    }
}
