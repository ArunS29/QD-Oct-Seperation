using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;
using System;
using System.Drawing;

namespace QD.ERP.Web.Areas.Finance.Reports.Payable_Statements
{
    public partial class EndDate : XtraReport
    {
        private SqlDataSource sqlDataSource1;

        public EndDate(string accountId, DateTime frmDate, DateTime toDate, string tenantName, string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb)
        {
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate, tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb);
        }

        public EndDate()
        {
            InitializeComponent();
            SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "", null, "", "");
        }

        private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate, string tenantName, string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb)
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
            AddOrUpdateParameter("CompanyName", company_Name ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", company_address ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", Company_Name_Ar ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", company_address_arb ?? "", typeof(string), false);

            // Debug: Ensure logo presence
            Console.WriteLine($"Company Logo Assigned: {logoImage != null}");

            // Bind controls dynamically if they exist
            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
            {
                tenantLabel.Text = tenantName;
            }
            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
            {
                companyNameLabel.Text = company_Name;
            }
            if (FindControl("xrLabelCompanyAddress", true) is XRLabel companyAddressLabel)
            {
                companyAddressLabel.Text = company_address;
            }
            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
            {
                companyNameArLabel.Text = Company_Name_Ar;
            }
            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel companyAddressArbLabel)
            {
                companyAddressArbLabel.Text = company_address_arb;
            }
            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
            {
                logoPictureBox.Image = logoImage;
            }
        }
    }
}
