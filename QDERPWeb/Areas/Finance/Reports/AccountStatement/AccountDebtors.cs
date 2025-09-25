using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using QD.ERP.Web.Service;
using System;
using System.Drawing;

namespace QD.ERP.Web.Areas.Finance.Reports.AccountStatement
{
    public partial class AccountDebtors : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public AccountDebtors(
            string accountId, DateTime frmDate, DateTime toDate,
            string tenantName, string company_Name, string company_address,
            Image logoImage, Image sealImage,
            string Company_Name_Ar, string company_address_arb,
            TenantDbContextHelper tenantDbContextHelper,
            string auditorName = "", string auditorAddress = "", string auditorEmail = "", string auditorFaxNo = "")
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            InitializeComponent();

            SetReportParameters(
                accountId, frmDate, toDate, tenantName,
                company_Name, company_address, logoImage, sealImage,
                Company_Name_Ar, company_address_arb,
                auditorName, auditorAddress, auditorEmail, auditorFaxNo
            );

            try
            {
                this.sqlDataSource1.Fill();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading report data: " + ex.Message, ex);
            }
        }

        public AccountDebtors()
        {
            InitializeComponent();
            SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "", null, null, "", "", "", "", "","");
        }

        private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate,
            string tenantName, string company_Name, string company_address,
            Image logoImage, Image sealImage,
            string Company_Name_Ar, string company_address_arb,
            string auditorName, string auditorAddress, string auditorEmail,string auditorFaxNo)
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

            // Existing parameters
            AddOrUpdateParameter("AccountID", accountId ?? "", typeof(string));
            AddOrUpdateParameter("StartDate", frmDate == DateTime.MinValue ? DateTime.Today : frmDate, typeof(DateTime));
            AddOrUpdateParameter("EndDate", toDate == DateTime.MinValue ? DateTime.Today : toDate, typeof(DateTime));
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", company_Name ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", company_address ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", Company_Name_Ar ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressArb", company_address_arb ?? "", typeof(string));

            // New Parameters
            AddOrUpdateParameter("AuditorName", auditorName ?? "", typeof(string));
            AddOrUpdateParameter("AuditorAddress", auditorAddress ?? "", typeof(string));
            AddOrUpdateParameter("AuditorEmail", auditorEmail ?? "", typeof(string));
            AddOrUpdateParameter("AuditorFax", auditorFaxNo ?? "", typeof(string));


            // Assign controls (if they exist in .repx)
            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = company_Name;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = company_address;

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;

            if (FindControl("xrPictureBox3", true) is XRPictureBox sealPictureBox)
                sealPictureBox.Image = sealImage;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = Company_Name_Ar;

            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = company_address_arb;

            if (FindControl("xrLabel13", true) is XRLabel auditorNameLabel)
                auditorNameLabel.Text = auditorName;

            if (FindControl("xrLabel21", true) is XRLabel auditorAddressLabel)
                auditorAddressLabel.Text = auditorAddress;

            if (FindControl("xrLabel22", true) is XRLabel auditorEmailLabel)
                auditorEmailLabel.Text = auditorEmail;

            AddSqlQueryParameters(accountId);
        }

        private void AddSqlQueryParameters(string accountId)
        
        {
            var sqlQuery = new CustomSqlQuery
            {
                Name = "qry201_101mainVoucherEntriesForAcctBalance",
                Sql = "SELECT * FROM qry201_101mainVoucherEntriesForAcctBalance WHERE AccountHead = @AccountHead"
            };

            sqlQuery.Parameters.Add(new QueryParameter("@AccountHead", typeof(string), accountId ?? "L00567"));

            sqlDataSource1.Queries.Clear();
            sqlDataSource1.Queries.Add(sqlQuery);
            sqlDataSource1.Name = "sqlDataSource1";

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

        private void xrPictureBox3_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
