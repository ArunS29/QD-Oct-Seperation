using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace QD.ERP.Web.Reports
{
    public partial class StatementOfAccountReport : DevExpress.XtraReports.UI.XtraReport
    {
        public StatementOfAccountReport(string accountId, DateTime frmDate, DateTime toDate, string tenantName, string companyName, string logoUrl)
        {
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate, tenantName, companyName, logoUrl);
        }

        public StatementOfAccountReport()
        {
            InitializeComponent();
            SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "");
        }

        private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate, string tenantName, string companyName, string logoUrl)
        {
            // Helper method to add or update a parameter
            void AddOrUpdateParameter(string name, object value, Type type, bool visible = false)
            {
                if (Parameters[name] == null)
                {
                    Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter()
                    {
                        Name = name,
                        Type = type,
                        Value = value,
                        Visible = visible
                    });
                }
                else
                {
                    Parameters[name].Value = value;
                    Parameters[name].Visible = visible;
                }
            }

            // Add or update parameters
            AddOrUpdateParameter("AccountID", accountId ?? "", typeof(string));
            AddOrUpdateParameter("StartDate", frmDate == DateTime.MinValue ? DateTime.Today : frmDate, typeof(DateTime));
            AddOrUpdateParameter("EndDate", toDate == DateTime.MinValue ? DateTime.Today : toDate, typeof(DateTime));

            // New parameters for Tenant and Company Info
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string), false);
            AddOrUpdateParameter("LogoUrl", logoUrl ?? "", typeof(string), false);

            // Debug: Ensure logo URL is captured
            Console.WriteLine($"Logo URL: {logoUrl}");

            // Bind the logo from URL if exists
            if (this.FindControl("xrPictureBoxLogo", true) is XRPictureBox logoControl)
            {
                // Ensure the URL is valid and non-empty
                if (!string.IsNullOrEmpty(logoUrl) && Uri.IsWellFormedUriString(logoUrl, UriKind.RelativeOrAbsolute))
                {
                    logoControl.ImageUrl = logoUrl;
                }
                else
                {
                    logoControl.ImageUrl = "/images/default-logo.png"; 
                }
            }

            // Bind TenantName and CompanyName to labels (update with actual control names)
            if (this.FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
            {
                tenantLabel.Text = tenantName;
            }

            if (this.FindControl("xrLabelCompanyName", true) is XRLabel companyLabel)
            {
                companyLabel.Text = companyName;
            }
        }
    }
}
