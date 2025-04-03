using System;
using System.Drawing;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;

namespace QD.ERP.Web.Areas.Finance.Reports.Cost_Analysis
{
    public partial class CostCenterSummaryReport : XtraReport
    {
        public CostCenterSummaryReport(
            string requestedBy,
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressAr)
        {
            InitializeComponent();
            SetReportParameters(requestedBy, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
        }

        public CostCenterSummaryReport()
        {
            InitializeComponent();
            SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "", null, "", "");
        }

        private void SetReportParameters(
            string requestedBy, DateTime frmDate, DateTime toDate,
            string tenantName, string companyName, string companyAddress,
            Image logoImage, string companyNameAr, string companyAddressAr)
        {
            void AddOrUpdateParameter(string name, object value, Type type, bool visible = false)
            {
                if (Parameters[name] == null)
                {
                    Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter()
                    {
                        Name = name,
                        Type = type,
                        Value = value,
                        Visible = false
                    });
                }
                else
                {
                    Parameters[name].Value = value;
                    Parameters[name].Visible = false;
                }
            }

            // Add report parameters
            AddOrUpdateParameter("RequestedBy", string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy, typeof(string), !string.IsNullOrEmpty(requestedBy));
            AddOrUpdateParameter("FrmDate", frmDate == DateTime.MinValue ? DateTime.Today : frmDate, typeof(DateTime));
            AddOrUpdateParameter("ToDate", toDate == DateTime.MinValue ? DateTime.Today : toDate, typeof(DateTime));
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressAr", companyAddressAr ?? "", typeof(string));

            // Bind parameters to UI controls
            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = companyName;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = companyAddress;

            if (FindControl("xrPictureBoxLogo", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = companyNameAr;

            if (FindControl("xrLabelCompanyAddressAr", true) is XRLabel addressArLabel)
                addressArLabel.Text = companyAddressAr;

            if (FindControl("xrLabelRequestedBy", true) is XRLabel requestedByLabel)
            {
                requestedByLabel.Text = string.IsNullOrEmpty(requestedBy) ? "" : requestedBy;
                requestedByLabel.Visible = !string.IsNullOrEmpty(requestedBy);
            }

            // Set up SQL query if needed
            AddSqlQueryParameters(requestedBy, frmDate, toDate);
        }

        private void AddSqlQueryParameters(string requestedBy, DateTime frmDate, DateTime toDate)
        {
            CustomSqlQuery selectQuery = new CustomSqlQuery()
            {
                Name = "qry20151CostAnalysisReport", // Update name
                Sql = @"SELECT * FROM qry20151CostAnalysisReport
    WHERE 
    (@RequestedBy IS NULL OR @RequestedBy = '' OR @RequestedBy = 'N/A' OR CostAllocationUnit = @RequestedBy) 
    AND VoucherDate BETWEEN @StartDate AND @EndDate"
            };


            selectQuery.Parameters.Add(new QueryParameter()
            {
                Name = "@RequestedBy",
                Type = typeof(string),
                ValueInfo = string.IsNullOrEmpty(requestedBy) || requestedBy == "N/A" ? "" : requestedBy
            });

            selectQuery.Parameters.Add(new QueryParameter()
            {
                Name = "@StartDate",
                Type = typeof(DateTime),
                ValueInfo = frmDate.ToString("yyyy-MM-dd")
            });

            selectQuery.Parameters.Add(new QueryParameter()
            {
                Name = "@EndDate",
                Type = typeof(DateTime),
                ValueInfo = toDate.ToString("yyyy-MM-dd")
            });

            this.sqlDataSource1.Queries.Clear();
            this.sqlDataSource1.Queries.Add(selectQuery);
            this.sqlDataSource1.Fill();
        }


    }
}
