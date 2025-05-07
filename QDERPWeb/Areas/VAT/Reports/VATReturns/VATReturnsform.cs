using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;

namespace QD.ERP.Web.Areas.VAT.Reports.VATReturns
{
	public partial class VATReturnsform : DevExpress.XtraReports.UI.XtraReport
    {

        private readonly TenantDbContextHelper _tenantDbContextHelper;
        public VATReturnsform(DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressAr,
            string username,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            InitializeComponent();
            SetReportParameters(frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username);
        }

        private void SetReportParameters(
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string username,
            string companyAddressAr)
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
                        Visible = false
                    });
                }
                else
                {
                    Parameters[name].Value = value;
                    Parameters[name].Visible = false;
                }
            }

            AddOrUpdateParameter("StartDate", frmDate, typeof(DateTime));
            AddOrUpdateParameter("EndDate", toDate, typeof(DateTime));
            AddOrUpdateParameter("UserName", username ?? "", typeof(string), false);
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressAr", companyAddressAr ?? "", typeof(string));

            if (FindControl("xrLabelUserName", true) is XRLabel userNameLabel)
                userNameLabel.Text = username;

            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = companyName;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = companyAddress;

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = companyNameAr;

            if (FindControl("xrLabelCompanyAddressAr", true) is XRLabel addressArLabel)
                addressArLabel.Text = companyAddressAr;

            ConfigureSqlDataSource(frmDate, toDate);
        }


        private void ConfigureSqlDataSource(DateTime frmDate, DateTime toDate)
        {
            var storedProcQuery = new StoredProcQuery()
            {
                Name = "sp201_999VATFinalReturns", 
                StoredProcName = "sp201_999VATFinalReturns"
            };

            storedProcQuery.Parameters.AddRange(new[]
            {
        new QueryParameter()
        {
            Name = "@StartDate",
            Type = typeof(DateTime),
            ValueInfo = frmDate.ToString("yyyy-MM-dd")
        },
        new QueryParameter()
        {
            Name = "@EndDate",
            Type = typeof(DateTime),
            ValueInfo = toDate.ToString("yyyy-MM-dd")
        }
    });

            this.sqlDataSource1.Queries.Clear();
            this.sqlDataSource1.Queries.Add(storedProcQuery);

            if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                var connectionParams = new CustomStringConnectionParameters(tenant.ConnectionString);
                this.sqlDataSource1.ConnectionParameters = connectionParams;
            }
            else
            {
                throw new Exception("Unable to get tenant context. Please check session and cache.");
            }

            this.sqlDataSource1.Fill();
        }


    }
}
