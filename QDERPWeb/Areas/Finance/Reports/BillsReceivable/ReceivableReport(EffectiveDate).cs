using System.Drawing;
using DevExpress.XtraReports.UI;

namespace QD.ERP.Web.Areas.Finance.Reports
{
    public partial class ReceivableReport_EffectiveDate_ : DevExpress.XtraReports.UI.XtraReport
    {
        public ReceivableReport_EffectiveDate_(string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb)
        {
            InitializeComponent();
            SetReportParameters(company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb);
        }
        public ReceivableReport_EffectiveDate_()
        {
            InitializeComponent();
            SetReportParameters("", "", null, "", "");
        }
        private void SetReportParameters(string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb)
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
            AddOrUpdateParameter("CompanyName", company_Name ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", company_address ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", Company_Name_Ar ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", company_address_arb ?? "", typeof(string), false);

            // Bind Company details to labels
            if (this.FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
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
