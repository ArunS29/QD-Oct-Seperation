using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using Svg;

namespace QD.ERP.Web.Reports
{
    public partial class AccountWithNarration : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public AccountWithNarration(
            string accountId,
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressArb,
            string username,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            InitializeComponent();

            SetReportParameters(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb, username);

            try
            {
                // Fill after configuring datasource and parameters
                sqlDataSource2.Fill();

                // Load currency image from filled data
                LoadCurrencyImage();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data: " + ex.Message, ex);
            }
        }

        public AccountWithNarration()
        {
            InitializeComponent();
        }

        private void SetReportParameters(string accountId, DateTime frmDate, DateTime toDate, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressArb, string username)
        {
            accountId ??= "";
            frmDate = frmDate == DateTime.MinValue ? DateTime.Today : frmDate;
            toDate = toDate == DateTime.MinValue ? DateTime.Today : toDate;

            AddOrUpdateParameter("AccountID", accountId, typeof(string), false);
            AddOrUpdateParameter("StartDate", frmDate, typeof(DateTime), false);
            AddOrUpdateParameter("EndDate", toDate, typeof(DateTime), false);
            AddOrUpdateParameter("UserName", username ?? "", typeof(string), false);
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string), false);

            // Set UI controls text/images
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

            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = companyAddressArb;

            ConfigureSqlDataSource(accountId, frmDate, toDate);
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

        private void ConfigureSqlDataSource(string accountId, DateTime frmDate, DateTime toDate)
        {
            sqlDataSource2.Queries.Clear();

            if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                // Set the connection string dynamically based on tenant
                sqlDataSource2.ConnectionParameters = new CustomStringConnectionParameters(tenant.ConnectionString);

                string schemaName = string.IsNullOrWhiteSpace(tenant.schemaname) ? "dbo" : tenant.schemaname;
                string fullStoredProcName = $"{schemaName}.StProAccountLedger";

                var storedProcQuery = new StoredProcQuery
                {
                    Name = "StProAccountLedger",
                    StoredProcName = fullStoredProcName
                };

                storedProcQuery.Parameters.AddRange(new[]
                {
                    new QueryParameter("@ParamAccountNo", typeof(string), accountId),
                    new QueryParameter("@StartDate", typeof(DateTime), frmDate),
                    new QueryParameter("@EndDate", typeof(DateTime), toDate)
                });

                sqlDataSource2.Queries.Add(storedProcQuery);
                sqlDataSource2.Name = "sqlDataSource2";
            }
            else
            {
                throw new Exception("Unable to get tenant context. Please check session and cache.");
            }
        }

        private void LoadCurrencyImage()
        {
            try
            {
                var resultSet = sqlDataSource2.Result;

                
                if (resultSet == null || !resultSet.Any(r => r.Name == "StProAccountLedger"))
                {
                    SetCurrencyImageNull();
                    return;
                }

                var data = resultSet.FirstOrDefault(r => r.Name == "StProAccountLedger") as System.Data.DataTable;


                if (data != null && data.Rows.Count > 0)
                {
                    var firstRow = data.Rows[0];

                    if (firstRow.Table.Columns.Contains("CurrencyImage") && firstRow["CurrencyImage"] != DBNull.Value)
                    {
                        byte[] imageBytes = (byte[])firstRow["CurrencyImage"];

                        try
                        {
                            // Try load as normal image (e.g., PNG, JPEG)
                            using (var ms = new MemoryStream(imageBytes))
                            {
                                ms.Seek(0, SeekOrigin.Begin);
                                var image = Image.FromStream(ms);

                                if (FindControl("xrPictureBox2", true) is XRPictureBox pictureBox)
                                    pictureBox.Image = image;
                            }
                        }
                        catch
                        {
                            // Fallback: treat bytes as SVG XML text
                            try
                            {
                                string svgText = Encoding.UTF8.GetString(imageBytes);

                                Image svgImage = ConvertSvgToImage(svgText);

                                if (FindControl("xrPictureBox2", true) is XRPictureBox pictureBox)
                                    pictureBox.Image = svgImage;
                            }
                            catch
                            {
                                SetCurrencyImageNull();
                            }
                        }
                    }
                    else
                    {
                        SetCurrencyImageNull();
                    }
                }
                else
                {
                    SetCurrencyImageNull();
                }
            }
            catch
            {
                SetCurrencyImageNull();
            }
        }

        private void SetCurrencyImageNull()
        {
            if (FindControl("xrPictureBox2", true) is XRPictureBox pictureBox)
                pictureBox.Image = null;
        }

        private Image ConvertSvgToImage(string svgString)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgString)))
            {
                var options = new SvgOptions();
                var svgDocument = SvgDocument.Open<SvgDocument>(stream, options);
                return svgDocument.Draw();
            }
        }
    }
}
