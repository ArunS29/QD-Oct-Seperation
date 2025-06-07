using System;
using System.Drawing;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;

namespace QD.ERP.Web.Areas.Finance.Reports.TrialBalance
{
    public partial class balnceSheet : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public balnceSheet(
            string accountGroup,
            DateTime toDate,
            string tenantName,
            string company_Name,
            string company_address,
            Image logoImage,
            string Company_Name_Ar,
            string company_address_arb,
            TenantDbContextHelper tenantDbContextHelper,
             string username,
            bool isUseEffectiveDate = true )
        {
            _tenantDbContextHelper = tenantDbContextHelper;

            InitializeComponent();
            SetReportParameters(accountGroup, toDate, tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb, isUseEffectiveDate,username);

            try
            {
                sqlDataSource1.Fill();

                if (!string.IsNullOrEmpty(accountGroup))
                {
                    this.FilterString = $"[AccountGroup] = '{accountGroup.Replace("'", "''")}'";
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading data for Balance Sheet report: {ex.Message}", ex);
            }
        }

        public balnceSheet()
        {
            InitializeComponent();
        }

        private void SetReportParameters(
            string accountGroup,
            DateTime toDate,
            string tenantName,
            string company_Name,
            string company_address,
            Image logoImage,
            string Company_Name_Ar,
            string company_address_arb,
            bool isUseEffectiveDate,string username)
        {
            AddReportParameter("AccountGroup", typeof(string), accountGroup ?? "");
            AddReportParameter("IsUseEffectiveDate", typeof(bool), isUseEffectiveDate);
            AddReportParameter("EndDate", typeof(DateTime), toDate == DateTime.MinValue ? DateTime.Today : toDate);
            AddReportParameter("TenantName", typeof(string), tenantName ?? "");
            AddReportParameter("CompanyName", typeof(string), company_Name ?? "");
            AddReportParameter("CompanyAddress", typeof(string), company_address ?? "");
            AddReportParameter("CompanyNameAr", typeof(string), Company_Name_Ar ?? "");
            AddReportParameter("CompanyAddressArb", typeof(string), company_address_arb ?? "");
            AddReportParameter("UserName",  typeof(string), username ?? "");

            ApplyReportControls(tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb,username);
            ConfigureSqlQuery();
        }

        private void ApplyReportControls(
            string tenantName,
            string company_Name,
            string company_address,
            Image logoImage,
            string Company_Name_Ar,
            string company_address_arb,
            string username)
        {

            if (FindControl("xrLabelUserName", true) is XRLabel userNameLabel)
                userNameLabel.Text = username;

            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = company_Name;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = company_address;

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage ?? null;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = Company_Name_Ar;

            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = company_address_arb;
        }

        private void ConfigureSqlQuery()
        {
            if (_tenantDbContextHelper != null &&
                _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                sqlDataSource1.ConnectionParameters = new CustomStringConnectionParameters(tenant.ConnectionString);

                // Use schema from tenant, or default to dbo
                string schemaName = string.IsNullOrWhiteSpace(tenant.schemaname) ? "dbo" : tenant.schemaname;
                string fullStoredProcName = $"{schemaName}.sp20113BalanceSheet";

                var storedProcQuery = new StoredProcQuery
                {
                    Name = "sp20113BalanceSheet",
                    StoredProcName = fullStoredProcName
                };

                storedProcQuery.Parameters.AddRange(new[]
                {
            new QueryParameter
            {
                Name = "@EndDate",
                Type = typeof(DateTime),
                Value = Parameters["EndDate"].Value
            },
            new QueryParameter
            {
                Name = "@IsUseEffectiveDate",
                Type = typeof(bool),
                Value = Parameters["IsUseEffectiveDate"].Value
            }
        });

                sqlDataSource1.Queries.Clear();
                sqlDataSource1.Queries.Add(storedProcQuery);
                sqlDataSource1.Name = "sqlDataSource1";
            }
            else
            {
                throw new Exception("Unable to get tenant context. Please check session and cache.");
            }
        }


        private void AddReportParameter(string paramName, Type paramType, object paramValue)
        {
            if (Parameters[paramName] == null)
            {
                Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter
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

        private void xrLabel12_BeforePrint(object sender, CancelEventArgs e)
        {
            // Optional: Add dynamic logic here
        }
    }
}
