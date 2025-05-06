using System;
using System.Drawing;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;
using QD.ERP.Web.Service; // Needed for TenantDbContextHelper

namespace QD.ERP.Web.Reports
{
	public partial class AccountStatementFormat2Report : XtraReport
	{
		private QueryParameter queryParameter1;
		private QueryParameter queryParameter2;
		private QueryParameter queryParameter3;
		private StoredProcQuery storedProcQuery1;

		private readonly TenantDbContextHelper _tenantDbContextHelper;

		public AccountStatementFormat2Report(
			string accountId,
			DateTime frmDate,
			DateTime toDate,
			string tenantName,
			string company_Name,
			string company_address,
			Image logoImage,
			string Company_Name_Ar,
			string company_address_arb,
			string username,
			TenantDbContextHelper tenantDbContextHelper
		)
		{
			_tenantDbContextHelper = tenantDbContextHelper;
			InitializeComponent();
			SetReportParameters(accountId, frmDate, toDate, tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb,username);

			try
			{
				this.sqlDataSource1.Fill();
			}
			catch (Exception ex)
			{
				throw new Exception("Error loading data: " + ex.Message, ex);
			}
		}

		public AccountStatementFormat2Report()
		{
			InitializeComponent();
			SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "", null, "", "","");
		}

		private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate, string tenantName, string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb,string username)
		{
			AddReportParameter("AccountID", typeof(string), accountId ?? "");
			AddReportParameter("StartDate", typeof(DateTime), frmDate == DateTime.MinValue ? DateTime.Today : frmDate);
			AddReportParameter("EndDate", typeof(DateTime), toDate == DateTime.MinValue ? DateTime.Today : toDate);
			AddSqlQueryParameters(accountId, frmDate, toDate);

			AddReportParameter("TenantName", typeof(string), tenantName ?? "");
            AddReportParameter("UserName", typeof(string), username ?? "");
            AddReportParameter("CompanyName", typeof(string), company_Name ?? "");
			AddReportParameter("CompanyAddress", typeof(string), company_address ?? "");
			AddReportParameter("CompanyNameAr", typeof(string), Company_Name_Ar ?? "");
			AddReportParameter("CompanyAddressArb", typeof(string), company_address_arb ?? "");

			Console.WriteLine($"Company Logo: {logoImage != null}");

			if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
				tenantLabel.Text = tenantName;
            if (FindControl("xrLabelUserName", true) is XRLabel userLabel)
                userLabel.Text = username;

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
		}

		private void AddReportParameter(string paramName, Type paramType, object paramValue)
		{
			if (Parameters[paramName] == null)
			{
				Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter()
				{
					Name = paramName,
					Type = paramType,
					Value = paramValue,
					Visible = false
				});
			}
			else
			{
				Parameters[paramName].Value = paramValue;
				Parameters[paramName].Visible = false;
			}
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

			// Connection string logic
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
