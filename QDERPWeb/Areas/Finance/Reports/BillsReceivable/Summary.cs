using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;

namespace QD.ERP.Web.Areas.Finance.Reports.BillsReceivable
{
	public partial class Summary : DevExpress.XtraReports.UI.XtraReport
	{
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        
       public Summary(
              string username,
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
            SetReportParameters(username,tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb);

            try
            {
                ConfigureSqlDataSource(); // Connect with tenant DB
                sqlDataSource1.Fill();    // Load data
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data: " + ex.Message, ex);
            }
        }

        public Summary()
        {
            InitializeComponent();
           
        }

        private void SetReportParameters(string username, string tenantName, string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb)
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

            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyName", company_Name ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", company_address ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", Company_Name_Ar ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", company_address_arb ?? "", typeof(string), false);
            AddOrUpdateParameter("UserName", username ?? "", typeof(string), false);
            if (FindControl("xrLabelUserName", true) is XRLabel userNameLabel)
                userNameLabel.Text = username;

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
        }

        private void ConfigureSqlDataSource()
        {
            sqlDataSource1.Queries.Clear();

           
            var customQuery = new CustomSqlQuery
            {
                Name = "qry20156BillsPayableSummaryMaster",
                Sql = "SELECT * FROM qry20156BillsPayableSummaryMaster"
            };

            sqlDataSource1.Queries.Add(customQuery);

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
