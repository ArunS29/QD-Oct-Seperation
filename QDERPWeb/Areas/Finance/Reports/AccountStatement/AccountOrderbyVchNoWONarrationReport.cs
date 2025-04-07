using System;
using System.Drawing;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;
using QD.ERP.Web.Service; // For TenantDbContextHelper

namespace QD.ERP.Web.Reports
{
	public partial class AccountOrderbyVchNoWONarrationReport : DevExpress.XtraReports.UI.XtraReport
	{
		private QueryParameter queryParameter1;
		private QueryParameter queryParameter2;
		private QueryParameter queryParameter3;
		private StoredProcQuery storedProcQuery1;
		private readonly TenantDbContextHelper _tenantDbContextHelper;

		// Constructor with TenantDbContextHelper
		public AccountOrderbyVchNoWONarrationReport(
			string accountId,
			DateTime frmDate,
			DateTime toDate,
			string tenantName,
			string company_Name,
			string company_address,
			Image logoImage,
			string Company_Name_Ar,
			string company_address_arb,
			TenantDbContextHelper tenantDbContextHelper)
		{
			_tenantDbContextHelper = tenantDbContextHelper;
			InitializeComponent();
			SetReportParameters(accountId, frmDate, toDate, tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb);

			try
			{
				this.sqlDataSource1.Fill();
			}
			catch (Exception ex)
			{
				throw new Exception("Error loading data: " + ex.Message, ex);
			}
		}

		// Parameterless constructor for designer support
		public AccountOrderbyVchNoWONarrationReport()
		{
			InitializeComponent();
			SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "", null, "", "");
		}

		private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate, string tenantName, string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb)
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

			if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
				tenantLabel.Text = tenantName;

			if (FindControl("xrLabelCompanyAddress", true) is XRLabel companyNameLabel)
				companyNameLabel.Text = company_Name;

			if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
				addressLabel.Text = company_address;

			if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
				logoPictureBox.Image = logoImage;

			if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
				companyNameArLabel.Text = Company_Name_Ar;

			if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
				addressArbLabel.Text = company_address_arb;

			AddSqlQueryParameters(accountId, frmDate, toDate);
		}

		private void AddSqlQueryParameters(string accountId, DateTime frmDate, DateTime toDate)
		{
			queryParameter1 = new QueryParameter
			{
				Name = "@ParamAccountNo",
				Type = typeof(string),
				ValueInfo = accountId ?? "L00567"
			};

			queryParameter2 = new QueryParameter
			{
				Name = "@StartDate",
				Type = typeof(DateTime),
				ValueInfo = (frmDate == DateTime.MinValue ? DateTime.Today : frmDate).ToString("yyyy-MM-dd")
			};

			queryParameter3 = new QueryParameter
			{
				Name = "@EndDate",
				Type = typeof(DateTime),
				ValueInfo = (toDate == DateTime.MinValue ? DateTime.Today : toDate).ToString("yyyy-MM-dd")
			};

			storedProcQuery1 = new StoredProcQuery
			{
				Name = "StProAccountLedger",
				StoredProcName = "StProAccountLedger"
			};

			storedProcQuery1.Parameters.Clear();
			storedProcQuery1.Parameters.AddRange(new QueryParameter[] { queryParameter1, queryParameter2, queryParameter3 });

			sqlDataSource1.Queries.Clear();
			sqlDataSource1.Queries.Add(storedProcQuery1);
			sqlDataSource1.Name = "sqlDataSource1";

			// Multi-tenant connection string setup
			if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
			{
				var connectionString = tenant.ConnectionString;
				var connectionParams = new CustomStringConnectionParameters(connectionString);
				sqlDataSource1.ConnectionParameters = connectionParams;
			}
			else
			{
				throw new Exception("Unable to get tenant context. Please check session and cache.");
			}
		}
	}
}
