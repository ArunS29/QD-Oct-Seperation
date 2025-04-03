using System.Drawing;
using DevExpress.XtraReports.UI;

namespace QD.ERP.Web.Areas.Finance.Reports
{
	public partial class ReceivableReport_EffectiveDate_ : DevExpress.XtraReports.UI.XtraReport
	{
		public ReceivableReport_EffectiveDate_(string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressArb)
		{
			InitializeComponent();
			SetCompanyDetails(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb);
		}

		public ReceivableReport_EffectiveDate_()
		{
			InitializeComponent();
			SetCompanyDetails("", "", "", null, "", "");
		}

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

			// Bind company details to report controls
			if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
				tenantLabel.Text = tenantName;

			if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
				companyNameLabel.Text = companyName;

			if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
				addressLabel.Text = companyAddress;

			if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox && logoImage != null)
				logoPictureBox.Image = logoImage;

			if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
				companyNameArLabel.Text = companyNameAr;

			if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
				addressArbLabel.Text = companyAddressArb;
		}
	}
}
