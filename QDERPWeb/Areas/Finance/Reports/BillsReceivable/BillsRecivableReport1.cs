using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Parameters;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;
using System;
using System.Drawing;
using QD.ERP.Web.Service; // For TenantDbContextHelper

namespace QD.ERP.Web.Areas.Finance.Reports.BillsReceivable
{
    public partial class BillsRecivableReport1 : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public BillsRecivableReport1(
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
            SetReportParameters(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb);
        }

        public BillsRecivableReport1()
        {
            InitializeComponent();
            SetReportParameters("", "", "", null, "", "");
        }

        private void SetReportParameters(string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressArb)
        {
            void AddOrUpdateParameter(string name, object value, Type type, bool visible = false)
            {
                if (Parameters[name] == null)
                {
                    Parameters.Add(new Parameter()
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

            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string));

            SetLabelText("xrLabelTenantName", tenantName);
            SetLabelText("xrLabelCompanyName", companyName);
            SetLabelText("xrLabelCompanyAddress", companyAddress);
            SetLabelText("xrLabelCompanyNameAr", companyNameAr);
            SetLabelText("xrLabelCompanyAddressArb", companyAddressArb);

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
            {
                logoPictureBox.Image = logoImage;
                logoPictureBox.Visible = logoImage != null;
            }

            LoadReportData(); // load full static data
        }

        private void SetLabelText(string controlName, string text)
        {
            if (FindControl(controlName, true) is XRLabel label)
            {
                label.Text = text ?? "";
            }
        }

        private void LoadReportData()
        {
            try
            {
                if (this.sqlDataSource1 == null)
                    this.sqlDataSource1 = new SqlDataSource();

                this.sqlDataSource1.Queries.Clear();

                var selectQuery = new CustomSqlQuery
                {
                    Name = "qry201SubLedgerReceivablesMaster",
                    Sql = "SELECT * FROM qry201SubLedgerReceivablesMaster"
                };

                this.sqlDataSource1.Queries.Add(selectQuery);

                if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    var connectionParams = new CustomStringConnectionParameters(tenant.ConnectionString);
                    this.sqlDataSource1.ConnectionParameters = connectionParams;
                }
                else
                {
                    throw new Exception("Unable to get tenant context. Please check session and cache.");
                }

                this.sqlDataSource1.RebuildResultSchema();
                this.sqlDataSource1.Fill();

                this.DataSource = sqlDataSource1;
                this.DataMember = selectQuery.Name;

                Console.WriteLine("Static Report Data Loaded Successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Loading Static Report Data: {ex.Message}");
            }
        }

        protected override void OnDataSourceDemanded(EventArgs e)
        {
            base.OnDataSourceDemanded(e);
            Console.WriteLine("Static BillsReceivable Report DataSource Demanded.");
        }
    }
}
