using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using DevExpress.XtraReports.UI;
using DevExpress.XtraPrinting;
using DevExpress.Utils.Svg;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;
using QD.ERP.Web.Service; // Ensure this is imported for TenantDbContextHelper

namespace QD.ERP.Web.Reports
{
	public partial class StatementOfAccountReport : XtraReport
	{
		private readonly TenantDbContextHelper _tenantDbContextHelper;

		public StatementOfAccountReport(
			string accountId, DateTime frmDate, DateTime toDate,
			string tenantName, string company_Name, string company_address,
			Image logoImage, string Company_Name_Ar, string company_address_arb,
			TenantDbContextHelper tenantDbContextHelper)
		{
			_tenantDbContextHelper = tenantDbContextHelper;
			InitializeComponent();
			SetReportParameters(accountId, frmDate, toDate, tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb);

			ConfigureSqlDataSource(accountId, frmDate, toDate);
		}

		public StatementOfAccountReport()
		{
			InitializeComponent();
			SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "", null, "", "");
		}

		private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate,
										 string tenantName, string company_Name, string company_address,
										 Image logoImage, string Company_Name_Ar, string company_address_arb)
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
						Visible = visible
					});
				}
				else
				{
					Parameters[name].Value = value;
					Parameters[name].Visible = visible;
				}
			}

			AddOrUpdateParameter("AccountID", accountId ?? "", typeof(string));
			AddOrUpdateParameter("StartDate", frmDate == DateTime.MinValue ? DateTime.Today : frmDate, typeof(DateTime));
			AddOrUpdateParameter("EndDate", toDate == DateTime.MinValue ? DateTime.Today : toDate, typeof(DateTime));

			AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
			AddOrUpdateParameter("CompanyName", company_Name ?? "", typeof(string));
			AddOrUpdateParameter("CompanyAddress", company_address ?? "", typeof(string));
			AddOrUpdateParameter("CompanyNameAr", Company_Name_Ar ?? "", typeof(string));
			AddOrUpdateParameter("CompanyAddressArb", company_address_arb ?? "", typeof(string));

			Console.WriteLine($"Company Logo: {(logoImage != null ? "Exists" : "Not Provided")}");

			AssignLabelText("xrLabelTenantName", tenantName);
			AssignLabelText("xrLabelCompanyAddress", company_address);
			AssignLabelText("xrLabelCompanyNameAr", Company_Name_Ar);
			AssignLabelText("xrLabelCompanyAddressArb", company_address_arb);

			if (this.FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
			{
				logoPictureBox.Image = logoImage;
			}

			ApplyIconFormatting();
		}

		private void ConfigureSqlDataSource(string accountId, DateTime frmDate, DateTime toDate)
		{
			sqlDataSource1.Queries.Clear();

			var storedProcQuery = new StoredProcQuery
			{
				Name = "StProAccountLedger",
				StoredProcName = "StProAccountLedger"
			};

			storedProcQuery.Parameters.AddRange(new[]
			{
				new QueryParameter("@AccountID", typeof(string), accountId),
				new QueryParameter("@StartDate", typeof(DateTime), frmDate),
				new QueryParameter("@EndDate", typeof(DateTime), toDate)
			});

			sqlDataSource1.Queries.Add(storedProcQuery);

			sqlDataSource1.ConnectionParameters = GetConnectionParameters();

			try
			{
				sqlDataSource1.Fill();
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to fill data source: " + ex.Message, ex);
			}
		}

		private CustomStringConnectionParameters GetConnectionParameters()
		{
			if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
			{
				if (!string.IsNullOrWhiteSpace(tenant.ConnectionString))
				{
					return new CustomStringConnectionParameters(tenant.ConnectionString);
				}
				else
				{
					throw new Exception("Tenant connection string is empty.");
				}
			}
			else
			{
				throw new Exception("Unable to get tenant context. Please check session and cache.");
			}
		}

		private void AssignLabelText(string controlName, string text)
		{
			if (this.FindControl(controlName, true) is XRLabel label)
			{
				label.Text = text ?? "";
			}
		}

		private void ApplyIconFormatting()
		{
			List<XRLabel> labelsToModify = new List<XRLabel>();

			foreach (XRControl control in this.AllControls<XRControl>())
			{
				if (control is XRLabel label && label.Tag != null && label.Tag.ToString().ToLower() == "showicon")
				{
					labelsToModify.Add(label);
				}
			}

			foreach (var label in labelsToModify)
			{
				AddSvgImageNextToLabel(label);
			}
		}

		private void AddSvgImageNextToLabel(XRLabel label)
		{
			string webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
			string filePath = Path.Combine(webRootPath, "images", "sar 1.svg");

			SvgImage svg = SvgImage.FromFile(filePath);
			if (svg == null) return;

			float iconWidth = 15;
			float iconHeight = 15;
			float posX = label.LocationF.X - iconWidth - 5;
			float posY = label.LocationF.Y + (label.HeightF - iconHeight) / 2;

			XRPictureBox iconImage = new XRPictureBox
			{
				ImageSource = new ImageSource(svg),
				Sizing = ImageSizeMode.StretchImage,
				WidthF = iconWidth,
				HeightF = iconHeight,
				LocationF = new PointF(posX, posY),
				Borders = BorderSide.None
			};

			label.Parent.Controls.Add(iconImage);
		}
	}
}
