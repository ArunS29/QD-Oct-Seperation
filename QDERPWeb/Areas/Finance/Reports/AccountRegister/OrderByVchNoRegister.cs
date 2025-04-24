using System;
using System.Drawing;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.XtraReports.UI;
using QD.ERP.Web.Service; // Ensure this namespace is present for TenantDbContextHelper

namespace QD.ERP.Web.Areas.Finance.Reports.AccountRegister
{
    public partial class OrderByVchNoRegister : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        // Constructor with TenantDbContextHelper for multitenancya
        public OrderByVchNoRegister(
            string voucherType,
            DateTime frmDate,
            DateTime toDate,
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
            SetReportParameters(voucherType, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb);

            try
            {
                this.sqlDataSource1.Fill(); // Fetch data immediately
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data: " + ex.Message, ex);
            }
        }

        // Default constructor for designer
        public OrderByVchNoRegister()
        {
            InitializeComponent();
        }

        private void SetReportParameters(
            string voucherType,
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressArb)
        {
            voucherType ??= "DefaultType";
            frmDate = frmDate == DateTime.MinValue ? DateTime.Today : frmDate;
            toDate = toDate == DateTime.MinValue ? DateTime.Today : toDate;

            AddOrUpdateParameter("VoucherType", voucherType, typeof(string), false);
            AddOrUpdateParameter("StartDate", frmDate, typeof(DateTime), false);
            AddOrUpdateParameter("EndDate", toDate, typeof(DateTime), false);
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string), false);

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

            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = companyAddressArb;

            ConfigureSqlDataSource(voucherType, frmDate, toDate);
        }

        private void AddOrUpdateParameter(string paramName, object paramValue, Type paramType, bool visible)
        {
            var parameter = Parameters[paramName];
            if (parameter == null)
            {
                Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter
                {
                    Name = paramName,
                    Type = paramType,
                    Value = paramValue,
                    Visible = visible
                });
            }
            else
            {
                parameter.Value = paramValue;
                parameter.Visible = visible;
            }
        }

        private void ConfigureSqlDataSource(string voucherType, DateTime frmDate, DateTime toDate)
        {
            sqlDataSource1.Queries.Clear();

            var storedProcQuery = new StoredProcQuery
            {
                Name = "StProAccountLedgerByVoucherType",
                StoredProcName = "StProAccountLedgerByVoucherType"
            };

            storedProcQuery.Parameters.AddRange(new[]
            {
                new QueryParameter("@VoucherType", typeof(string), voucherType),
                new QueryParameter("@StartDate", typeof(DateTime), frmDate),
                new QueryParameter("@EndDate", typeof(DateTime), toDate)
            });

            sqlDataSource1.Queries.Add(storedProcQuery);

            if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                var connectionParams = new CustomStringConnectionParameters(tenant.ConnectionString);
                sqlDataSource1.ConnectionParameters = connectionParams;
            }
            else
            {
                throw new Exception("Unable to get tenant context. Please check session and cache.");
            }
        }
    }
}
