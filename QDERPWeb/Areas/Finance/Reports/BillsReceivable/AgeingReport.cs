using DevExpress.XtraReports.UI;
using System;
using System.Drawing;

namespace QD.ERP.Web.Areas.Finance.Reports.BillsReceivable
{
    public partial class AgeingReport : DevExpress.XtraReports.UI.XtraReport
    {
        // Constructor with company details only
        public AgeingReport(string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressArb)
        {
            InitializeComponent();
            SetCompanyDetails(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb);
        }

        // Parameterless constructor for design mode
        public AgeingReport()
        {
            InitializeComponent();
            SetCompanyDetails("", "", "", null, "", "");
        }

        // Method to set company details in the report
        private void SetCompanyDetails(string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressArb)
        {
            // Helper method to add or update a report parameter
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

            // Add company details parameters
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string), false);

            // Debug: Ensure logo is captured
            Console.WriteLine($"Company Logo: {logoImage != null}");

            // Bind company details to report controls
            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = companyName;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = companyAddress;

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = companyNameAr;

            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = companyAddressArb;
        }
    }
}
