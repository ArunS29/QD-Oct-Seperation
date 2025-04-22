using System;
using System.Drawing;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;

namespace QD.ERP.Web.Areas.Finance.Reports.TrialBalance
{
    public partial class BillsLedgerBalance : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        // Constructor with parameters for customization
        public BillsLedgerBalance(DateTime startDate,
                                  DateTime endDate,
                                  string tenantName,
                                  string companyName,
                                  string companyAddress,
                                  Image logoImage,
                                  string companyNameAr,
                                  string companyAddressArb,
                                  TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;

            InitializeComponent();
            SetReportParameters(startDate, endDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb);

            try
            {
                sqlDataSource1.Fill();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error loading data for BillsLedgerBalance report: {ex.Message}", ex);
            }
        }

        // Default constructor for design-time
        public BillsLedgerBalance()
        {
            InitializeComponent();
        }

        private void SetReportParameters(DateTime startDate,
                                         DateTime endDate,
                                         string tenantName,
                                         string companyName,
                                         string companyAddress,
                                         Image logoImage,
                                         string companyNameAr,
                                         string companyAddressArb)
        {
            AddReportParameter("StartDate", typeof(DateTime), startDate == DateTime.MinValue ? DateTime.Today : startDate);
            AddReportParameter("EndDate", typeof(DateTime), endDate == DateTime.MinValue ? DateTime.Today : endDate);
            AddReportParameter("TenantName", typeof(string), tenantName ?? "");
            AddReportParameter("CompanyName", typeof(string), companyName ?? "");
            AddReportParameter("CompanyAddress", typeof(string), companyAddress ?? "");
            AddReportParameter("CompanyNameAr", typeof(string), companyNameAr ?? "");
            AddReportParameter("CompanyAddressArb", typeof(string), companyAddressArb ?? "");

            ApplyReportControls(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb);
            ConfigureSqlQuery();
        }

        private void ApplyReportControls(string tenantName,
                                         string companyName,
                                         string companyAddress,
                                         Image logoImage,
                                         string companyNameAr,
                                         string companyAddressArb)
        {
            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelCompanyName", true) is XRLabel nameLabel)
                nameLabel.Text = companyName;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = companyAddress;

            if (logoImage != null && FindControl("xrPictureBox1", true) is XRPictureBox logoBox)
                logoBox.Image = logoImage;
            else if (FindControl("xrPictureBox1", true) is XRPictureBox defaultLogo)
                defaultLogo.Image = null;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel nameArLabel)
                nameArLabel.Text = companyNameAr;

            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = companyAddressArb;
        }

        private void ConfigureSqlQuery()
        {
            var storedProcQuery = new StoredProcQuery
            {
                Name = "sp20125AgeingReceivableReportsWtAdvances",
                StoredProcName = "sp20125AgeingReceivableReportsWtAdvances"
            };

            storedProcQuery.Parameters.AddRange(new[]
            {

                new QueryParameter { Name = "@EndDate", Type = typeof(DateTime), Value = Parameters["EndDate"].Value },

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

        private void AddReportParameter(string name, Type type, object value)
        {
            if (Parameters[name] == null)
            {
                Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter
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
    }
}

