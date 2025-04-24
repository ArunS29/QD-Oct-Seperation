using System;
using System.Drawing;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;

namespace QD.ERP.Web.Areas.Finance.Reports.Register
{
    public partial class Group : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public Group(string accountGroup,
                     DateTime frmDate,
                     DateTime toDate,
                     string tenantName,
                     string company_Name,
                     string company_address,
                     Image logoImage,
                     string Company_Name_Ar,
                     string company_address_arb,
                     TenantDbContextHelper tenantDbContextHelper,
                     bool isUseEffectiveDate = true)
        {
            _tenantDbContextHelper = tenantDbContextHelper;

            InitializeComponent();
            SetReportParameters(accountGroup, frmDate, toDate, tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb, isUseEffectiveDate);

            try
            {
                sqlDataSource1.Fill();

                // Apply filter after data load
                if (!string.IsNullOrEmpty(accountGroup))
                {
                    this.FilterString = $"[AccountGroup] = '{accountGroup}'";
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading data for Group report: {ex.Message}", ex);
            }
        }

        public Group()
        {
            InitializeComponent();
        }

        private void SetReportParameters(string accountGroup, DateTime frmDate, DateTime toDate, string tenantName, string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb, bool isUseEffectiveDate)
        {
            AddReportParameter("AccountGroup", typeof(string), accountGroup ?? "");
            AddReportParameter("StartDate", typeof(DateTime), frmDate == DateTime.MinValue ? DateTime.Today : frmDate);
            AddReportParameter("EndDate", typeof(DateTime), toDate == DateTime.MinValue ? DateTime.Today : toDate);
            AddReportParameter("TenantName", typeof(string), tenantName ?? "");
            AddReportParameter("CompanyName", typeof(string), company_Name ?? "");
            AddReportParameter("CompanyAddress", typeof(string), company_address ?? "");
            AddReportParameter("CompanyNameAr", typeof(string), Company_Name_Ar ?? "");
            AddReportParameter("CompanyAddressArb", typeof(string), company_address_arb ?? "");
            AddReportParameter("IsUseEffectiveDate", typeof(bool), isUseEffectiveDate);

            ApplyReportControls(tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb);
            ConfigureSqlQuery();
        }

        private void ApplyReportControls(string tenantName, string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb)
        {
            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = company_Name;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = company_address;

            if (logoImage != null && FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;
            else
            {
                // Handle the case when logoImage is null (use a default image or set an empty picture)
                if (FindControl("xrPictureBox1", true) is XRPictureBox defaultLogoPictureBox)
                    defaultLogoPictureBox.Image = null; // Or assign a default image
            }

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = Company_Name_Ar;

            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = company_address_arb;
        }

        private void ConfigureSqlQuery()
        {
            var storedProcQuery = new StoredProcQuery
            {
                Name = "sp20101TrialBalanceReport",
                StoredProcName = "sp20101TrialBalanceReport"
            };

            storedProcQuery.Parameters.AddRange(new[]
            {
                new QueryParameter { Name = "@StartDate", Type = typeof(DateTime), Value = Parameters["StartDate"].Value },
                new QueryParameter { Name = "@EndDate", Type = typeof(DateTime), Value = Parameters["EndDate"].Value },
                new QueryParameter { Name = "@IsUseEffectiveDate", Type = typeof(bool), Value = Parameters["IsUseEffectiveDate"].Value }
            });

            sqlDataSource1.Queries.Clear();
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

        private void xrLabel8_BeforePrint(object sender, CancelEventArgs e)
        {
        
        }
    }
}
