using System;
using System.Drawing;
using System.Linq;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Parameters;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;
using QD.ERP.Web.Service; // Needed for TenantDbContextHelper

namespace QD.ERP.Web.Areas.Finance.Reports
{
    public partial class XtraRecivableReport : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public XtraRecivableReport(
            object[] selectedValues,
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
            SetReportParameters(selectedValues, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb);
        }

        public XtraRecivableReport()
        {
            InitializeComponent();
            SetReportParameters(null, "", "", "", null, "", "");
        }

        private void SetReportParameters(object[] selectedValues, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressArb)
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
            AddOrUpdateParameter("SelectedValues", selectedValues ?? new object[0], typeof(object[]));

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

            // Multitenant-aware data loading
            LoadReportData(selectedValues);
        }

        private void SetLabelText(string controlName, string text)
        {
            if (FindControl(controlName, true) is XRLabel label)
                label.Text = text ?? "";
        }

        private void LoadReportData(object[] selectedValues)
        {
            if (selectedValues == null || selectedValues.Length == 0)
            {
                Console.WriteLine("No selection provided. Loading full report.");
                return;
            }

            string firstValue = selectedValues.First()?.ToString();
            if (string.IsNullOrWhiteSpace(firstValue))
            {
                Console.WriteLine("Invalid first value. Report data not loaded.");
                return;
            }

            string selectedFilter = string.Join(",", selectedValues.Select(val => $"'{val.ToString().Replace("'", "''")}'"));

            CustomSqlQuery selectQuery = new CustomSqlQuery();

            if (firstValue.StartsWith("L"))
            {
                selectQuery.Name = "qry201SubLedgerReceivablesMaster";
                selectQuery.Sql = $"SELECT * FROM qry201SubLedgerReceivablesMaster WHERE AccountHeadNo IN ({FormatSelectedFilter(selectedValues)})";
            }
            else if (IsSalesPersonCode(firstValue))
            {
                selectQuery.Name = "qry201SubLedgerReceivablesMaster";
                selectQuery.Sql = $"SELECT * FROM qry201SubLedgerReceivablesMaster WHERE SalesPersonCode IN ({selectedFilter})";
            }
            else if (IsBranchCode(firstValue))
            {
                selectQuery.Name = "qry201SubLedgerReceivablesMaster";
                selectQuery.Sql = $"SELECT * FROM qry201SubLedgerReceivablesMaster WHERE BranchCode IN ({selectedFilter})";
            }
            else
            {
                Console.WriteLine("Invalid selection type. No report generated.");
                return;
            }

            try
            {
                if (this.sqlDataSource1 == null)
                    this.sqlDataSource1 = new SqlDataSource();

                this.sqlDataSource1.Queries.Clear();
                this.sqlDataSource1.Queries.Add(selectQuery);

                // Multitenant database connection setup
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

                Console.WriteLine("Report Data Loaded Successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Loading Report Data: {ex.Message}");
            }
        }

        private string FormatSelectedFilter(object[] filters)
        {
            return string.Join(",", filters.Select(x => $"'{x.ToString().Trim().Replace("'", "''")}'"));
        }

        private bool IsSalesPersonCode(string value)
        {
            return int.TryParse(value, out int num) && (num >= 100 && num <= 999);
        }

        private bool IsBranchCode(string value)
        {
            return int.TryParse(value, out int num) && (num >= 1 && num <= 99);
        }

        protected override void OnDataSourceDemanded(EventArgs e)
        {
            base.OnDataSourceDemanded(e);
            Console.WriteLine("Report DataSource Demanded.");
        }
    }
}
