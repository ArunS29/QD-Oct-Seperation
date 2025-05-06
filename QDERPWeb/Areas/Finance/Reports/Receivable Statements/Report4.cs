using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;
using QD.ERP.Web.Service;
using System.Collections.Generic;

namespace QD.ERP.Web.Areas.Finance.Reports.Receivable_Statements
{
    public partial class Report4 : XtraReport
    {
        private const string QueryName = "qry201SubLedgerReceivablesMaster ";
        private readonly TenantDbContextHelper _tenantDbContextHelper;
      
        public Report4(
            string accountId,
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string company_Name,
            string company_address,
            Image logoImage,
            string Company_Name_Ar,
            string company_address_arb,
            string username,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate, tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb,username);
        }

        public Report4()
        {
            InitializeComponent();
            SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "", null, "", "","");
        }

        private void SetReportParameters(
            string accountId,
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string company_Name,
            string company_address,
            Image logoImage,
            string Company_Name_Ar,
            string username,
            string company_address_arb)
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

            AddOrUpdateParameter("AccountID", accountId ?? "", typeof(string));
            AddOrUpdateParameter("StartDate", frmDate == DateTime.MinValue ? DateTime.Today : frmDate, typeof(DateTime));
            AddOrUpdateParameter("EndDate", toDate == DateTime.MinValue ? DateTime.Today : toDate, typeof(DateTime));
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("UserName", username ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", company_Name ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", company_address ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", Company_Name_Ar ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressArb", company_address_arb ?? "", typeof(string));

            if (this.FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;
            if (this.FindControl("xrLabelUserName", true) is XRLabel userNameLabel)
                userNameLabel.Text = username;

            if (this.FindControl("xrLabelCompanyAddress", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = company_Name;

            if (this.FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = company_address;

            if (this.FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox && logoImage != null)
                logoPictureBox.Image = logoImage;

            if (this.FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = Company_Name_Ar;

            if (this.FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = company_address_arb;

            AddSqlQueryParameters(accountId, frmDate, toDate);
        }

        private void AddSqlQueryParameters(string accountId, DateTime frmDate, DateTime toDate)
        {
            if (_tenantDbContextHelper == null || !_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                throw new Exception("Unable to get tenant context. Please check session and cache.");

            var connectionParams = new CustomStringConnectionParameters(tenant.ConnectionString);
            sqlDataSource1 = new SqlDataSource(connectionParams);

            var query = new CustomSqlQuery
            {
                Name = QueryName,
                Sql = @"SELECT * FROM qry201SubLedgerReceivablesMaster  
                        WHERE (@AccountID IS NULL OR AccountHeadNo = @AccountID)
                        AND VoucherDate BETWEEN @StartDate AND @EndDate"
            };

            query.Parameters.Add(new QueryParameter
            {
                Name = "@AccountID",
                Type = typeof(string),
                ValueInfo = accountId ?? ""
            });

            query.Parameters.Add(new QueryParameter
            {
                Name = "@StartDate",
                Type = typeof(DateTime),
                ValueInfo = frmDate.ToString("yyyy-MM-dd")
            });

            query.Parameters.Add(new QueryParameter
            {
                Name = "@EndDate",
                Type = typeof(DateTime),
                ValueInfo = toDate.ToString("yyyy-MM-dd")
            });

            sqlDataSource1.Queries.Clear();
            sqlDataSource1.Queries.Add(query);
            sqlDataSource1.RebuildResultSchema(); // Optional but recommended
            sqlDataSource1.Fill();

            this.DataSource = sqlDataSource1;
            this.DataMember = QueryName;

            CheckForEmptyData();
        }

        private void CheckForEmptyData()
        {
            if (sqlDataSource1.Result[QueryName] is IList result && result.Count == 0)
            {
                XRLabel noDataLabel = new XRLabel()
                {
                    Text = "No records found to display.",
                    BoundsF = new RectangleF(0, 0, 650, 50),
                    TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter,
                    Font = new Font("Arial", 14, FontStyle.Bold)
                };

                this.Bands[BandKind.Detail].Controls.Add(noDataLabel);
            }
        }
    }
}
