using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace QD.ERP.Web.Reports
{
    public partial class StatementOfAccountReport : DevExpress.XtraReports.UI.XtraReport
    {
        // Constructor with parameters for when viewing the report
        public StatementOfAccountReport(string accountId, DateTime frmDate, DateTime toDate)
        {
            InitializeComponent();

            // Add or update the parameters for viewing the report
            SetReportParameters(accountId, frmDate, toDate);
        }

        // Parameterless constructor for the Report Designer
        public StatementOfAccountReport()
        {
            InitializeComponent();

            // Set default values or no parameters for design mode
            SetReportParameters(null, DateTime.MinValue, DateTime.MinValue);
        }

        private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate)
        {
            // AccountId Parameter
            if (Parameters["AccountID"] == null)
            {
                Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter()
                {
                    Name = "AccountID",
                    Type = typeof(string),
                    Value = accountId ?? "" // Empty string if accountId is null in design mode
                });
            }
            else
            {
                Parameters["AccountID"].Value = accountId ?? "";
            }

            // StartDate Parameter
            if (Parameters["StartDate"] == null)
            {
                Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter()
                {
                    Name = "StartDate",
                    Type = typeof(DateTime),
                    Value = frmDate == DateTime.MinValue ? DateTime.Today : frmDate // Default to today if no valid date for design mode
                });
            }
            else
            {
                Parameters["StartDate"].Value = frmDate == DateTime.MinValue ? DateTime.Today : frmDate;
            }

            // EndDate Parameter
            if (Parameters["EndDate"] == null)
            {
                Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter()
                {
                    Name = "EndDate",
                    Type = typeof(DateTime),
                    Value = toDate == DateTime.MinValue ? DateTime.Today : toDate // Default to today if no valid date for design mode
                });
            }
            else
            {
                Parameters["EndDate"].Value = toDate == DateTime.MinValue ? DateTime.Today : toDate;
            }
        }
    }
}
