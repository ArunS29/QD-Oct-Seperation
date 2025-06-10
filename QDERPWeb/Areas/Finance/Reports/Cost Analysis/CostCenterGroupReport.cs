using System;
using System.Drawing;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.XtraReports.UI;
using QD.ERP.Web.Service;
using DevExpress.XtraPrinting;
using Svg;
using System.Text;
using Microsoft.Data.SqlClient; // Make sure this is added

namespace QD.ERP.Web.Areas.Finance.Reports.Cost_Analysis
{
    public partial class CostCenterGroupReport : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public CostCenterGroupReport(
            string requestedBy,
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressAr,
            TenantDbContextHelper tenantDbContextHelper,string username)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            InitializeComponent();
            LoadCurrencySymbolAndImage();

            SetReportParameters(requestedBy, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username);
        }

        public CostCenterGroupReport()
        {
            InitializeComponent();
            SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "", null, "", "","");
        }

        private void SetReportParameters(
            string requestedBy, DateTime frmDate, DateTime toDate,
            string tenantName, string companyName, string companyAddress,
            Image logoImage, string companyNameAr, string companyAddressAr, string username)
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

            AddOrUpdateParameter("RequestedBy", string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy, typeof(string), !string.IsNullOrEmpty(requestedBy));
            AddOrUpdateParameter("FrmDate", frmDate == DateTime.MinValue ? DateTime.Today : frmDate, typeof(DateTime));
            AddOrUpdateParameter("ToDate", toDate == DateTime.MinValue ? DateTime.Today : toDate, typeof(DateTime));
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressAr", companyAddressAr ?? "", typeof(string));
            AddOrUpdateParameter("userName", username ?? "", typeof(string), false);
            if (FindControl("xrLabeluserName", true) is XRLabel userLabel)
                userLabel.Text = username;
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

            if (FindControl("xrLabelRequestedBy", true) is XRLabel requestedByLabel)
            {
                requestedByLabel.Text = string.IsNullOrEmpty(requestedBy) ? "" : requestedBy;
                requestedByLabel.Visible = !string.IsNullOrEmpty(requestedBy);
            }

            AddSqlQueryWithMultitenantConnection(requestedBy, frmDate, toDate);
        }

        private void AddSqlQueryWithMultitenantConnection(string requestedBy, DateTime frmDate, DateTime toDate)
        {
            var selectQuery = new CustomSqlQuery()
            {
                Name = "qry20151CostAnalysisReport",
                Sql = @"SELECT * FROM qry20151CostAnalysisReport
                        WHERE 
                        (@RequestedBy IS NULL OR @RequestedBy = '' OR @RequestedBy = 'N/A' OR CostAllocationUnit = @RequestedBy) 
                        AND VoucherDate BETWEEN @StartDate AND @EndDate"
            };

            selectQuery.Parameters.AddRange(new[]
            {
                new QueryParameter()
                {
                    Name = "@RequestedBy",
                    Type = typeof(string),
                    ValueInfo = string.IsNullOrEmpty(requestedBy) || requestedBy == "N/A" ? "" : requestedBy
                },
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

            sqlDataSource1.Queries.Clear();
            sqlDataSource1.Queries.Add(selectQuery);

            // Apply multitenant logic
            if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                var connectionParams = new CustomStringConnectionParameters(tenant.ConnectionString);
                sqlDataSource1.ConnectionParameters = connectionParams;
            }
            else
            {
                throw new Exception("Unable to get tenant context. Please check session and cache.");
            }

            sqlDataSource1.Fill();
        }
        private void LoadCurrencySymbolAndImage()
        {
            try
            {
                if (_tenantDbContextHelper == null || !_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    SetCurrencyImageNull();
                    return;
                }

                string connectionString = tenant.ConnectionString;
                string svgText = null;
                string currencySymbol = null;

                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = $"SELECT TOP 1 CurrencyImage, CurrencySymbol FROM {tenant.schemaname}.tbl901companyDetails";
                    using (var command = new SqlCommand(sql, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                svgText = reader["CurrencyImage"]?.ToString()?.Trim('\uFEFF');
                                currencySymbol = reader["CurrencySymbol"]?.ToString()?.Trim();
                            }
                        }
                    }
                }

                // Set currency symbol to label
                if (FindControl("xrLabelCurrencySymbol", true) is XRLabel currencyLabel && !string.IsNullOrEmpty(currencySymbol))
                {
                    currencyLabel.Text = currencySymbol;
                }

                if (string.IsNullOrWhiteSpace(svgText))
                {
                    SetCurrencyImageNull();
                    return;
                }

                Bitmap bitmap = null;
                try
                {
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgText)))
                    {
                        SvgDocument svgDoc = SvgDocument.Open<SvgDocument>(stream);
                        bitmap = svgDoc.Draw();
                    }
                }
                catch
                {
                    bitmap = null;
                }

                if (bitmap == null)
                {
                    SetCurrencyImageNull();
                    return;
                }

                string[] pictureBoxNames = { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9" };

                foreach (string name in pictureBoxNames)
                {
                    if (FindControl(name, true) is XRPictureBox pictureBox)
                    {
                        pictureBox.Image = bitmap;
                        pictureBox.Sizing = ImageSizeMode.Normal;
                    }
                }
            }
            catch
            {
                SetCurrencyImageNull();
            }
        }
        private void SetCurrencyImageNull()
        {
            string[] pictureBoxNames = { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9" };

            foreach (string name in pictureBoxNames)
            {
                if (FindControl(name, true) is XRPictureBox pictureBox)
                {
                    pictureBox.Image = null;
                    pictureBox.ImageSource = null;
                }
            }

            if (FindControl("xrLabelCurrencySymbol", true) is XRLabel currencyLabel)
            {
                currencyLabel.Text = "";
            }
        }
    }
}
