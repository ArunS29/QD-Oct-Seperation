using System.Drawing;
using DevExpress.XtraReports.UI;
using Microsoft.Identity.Client;

namespace QD.ERP.Web.Areas.Finance.Reports
{
    public partial class BIllsPayable : DevExpress.XtraReports.UI.XtraReport
    {

        public BIllsPayable(string tenantName, string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb)
        {
            InitializeComponent();
            SetReportParameters(tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb);
        }
        public BIllsPayable()
        {
            InitializeComponent();
            SetReportParameters("", "", "", null, "", "");
        }

        private void SetReportParameters(string tenantName, string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb)
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




            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyName", company_Name ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", company_address ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", Company_Name_Ar ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", company_address_arb ?? "", typeof(string), false);

            // Debug: Ensure logo URL is captured
            Console.WriteLine($"Company Logo: {logoImage != null}");


            // Bind TenantName and CompanyName to labels (update with actual control names)
            if (this.FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
            {
                tenantLabel.Text = tenantName;
            }
            if (this.FindControl("xrLabelCompanyAddress", true) is XRLabel companyNameLabel)
            {
                companyNameLabel.Text = company_Name;
            }

            if (this.FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
            {
                addressLabel.Text = company_address;
            }

            if (this.FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
            {
                logoPictureBox.Image = logoImage;
            }

            if (this.FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
            {
                companyNameArLabel.Text = Company_Name_Ar;
            }

            if (this.FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
            {
                addressArbLabel.Text = company_address_arb;
            }




        }
    }
}
