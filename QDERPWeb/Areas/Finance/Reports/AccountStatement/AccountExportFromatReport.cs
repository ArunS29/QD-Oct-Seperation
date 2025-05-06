using System;
using System.Drawing;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.XtraReports.UI;
using QD.ERP.Web.Service;

namespace QD.ERP.Web.Reports
{
    public partial class AccountExportFromatReport : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public AccountExportFromatReport(
            string accountId, DateTime frmDate, DateTime toDate,
            string tenantName, string company_Name, string company_address,
            Image logoImage, string Company_Name_Ar, string company_address_arb,
            string username, TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate, tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb, username);

            try
            {
                sqlDataSource1.Fill();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data: " + ex.Message, ex);
            }
        }

        public AccountExportFromatReport()
        {
            InitializeComponent();
        }

        private void SetReportParameters(
            string accountId, DateTime frmDate, DateTime toDate,
            string tenantName, string company_Name, string company_address,
            Image logoImage, string Company_Name_Ar, string company_address_arb, string username)
        {
            AddOrUpdateParameter("AccountID", accountId ?? "", typeof(string));
            AddOrUpdateParameter("StartDate", frmDate == DateTime.MinValue ? DateTime.Today : frmDate, typeof(DateTime));
            AddOrUpdateParameter("EndDate", toDate == DateTime.MinValue ? DateTime.Today : toDate, typeof(DateTime));
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("UserName", username ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", company_Name ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", company_address ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", Company_Name_Ar ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressArb", company_address_arb ?? "", typeof(string));

            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;
            if (FindControl("xrLabelUserName", true) is XRLabel userLabel)
                userLabel.Text = username;
            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = company_Name;
            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = company_address;
            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;
            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = Company_Name_Ar;
            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = company_address_arb;

            ConfigureSqlDataSource(accountId, frmDate, toDate);
        }

        private void AddOrUpdateParameter(string name, object value, Type type, bool visible = false)
        {
            if (Parameters[name] == null)
            {
                Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter
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
                new QueryParameter("@ParamAccountNo", typeof(string), accountId ?? ""),
                new QueryParameter("@StartDate", typeof(DateTime), frmDate == DateTime.MinValue ? DateTime.Today : frmDate),
                new QueryParameter("@EndDate", typeof(DateTime), toDate == DateTime.MinValue ? DateTime.Today : toDate)
            });

            sqlDataSource1.Queries.Add(storedProcQuery);
            sqlDataSource1.Name = "sqlDataSource1";

            if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                sqlDataSource1.ConnectionParameters = new CustomStringConnectionParameters(tenant.ConnectionString);
            }
            else
            {
                throw new Exception("Unable to get tenant context. Please check session and cache.");
            }
        }
    }
}
