using System;
using System.Drawing;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;
using QD.ERP.Web.Service;
using DevExpress.XtraPrinting;
using Svg;
using System.Text;

namespace QD.ERP.Web.Areas.Finance.Reports
{
    public partial class AccountOrderByVoucherNo : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public AccountOrderByVoucherNo(
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
            SetReportParameters(accountId, frmDate, toDate, tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb, username);

            try
            {
                sqlDataSource2.Fill();
                LoadCurrencyImage(accountId, frmDate, toDate);

            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data: " + ex.Message, ex);
            }
        }

        public AccountOrderByVoucherNo()
        {
            InitializeComponent();
            SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "", null, "", "", "");
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
            string company_address_arb,
            string username)
        {
            void AddOrUpdateParameter(string name, object value, Type type, bool visible = false)
            {
                if (Parameters[name] == null)
                {
                    Parameters.Add(new DevExpress.XtraReports.Parameters.Parameter
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
            AddOrUpdateParameter("CompanyName", company_Name ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", company_address ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", Company_Name_Ar ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressArb", company_address_arb ?? "", typeof(string));
            AddOrUpdateParameter("UserName", username ?? "", typeof(string));

            // Set control values
            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelUserName", true) is XRLabel userLabel)
                userLabel.Text = username;

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

            ConfigureSqlDataSource(accountId, frmDate, toDate);
        }

        private void ConfigureSqlDataSource(string accountId, DateTime frmDate, DateTime toDate)
        {
            sqlDataSource2.Queries.Clear();

            if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                sqlDataSource2.ConnectionParameters = new CustomStringConnectionParameters(tenant.ConnectionString);

                // Use schema from tenant, or default to dbo
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
        private void LoadCurrencyImage(string accountId, DateTime frmDate, DateTime toDate)
        {
            if (string.IsNullOrEmpty(accountId))
            {
                SetCurrencyImageNull();
                return;
            }

            try
            {
                if (_tenantDbContextHelper == null || !_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    SetCurrencyImageNull();
                    return;
                }

                string connectionString = tenant.ConnectionString;
                string svgText = null;

                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();
                    using (var command = new System.Data.SqlClient.SqlCommand($"{tenant.schemaname}.StProAccountLedger", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ParamAccountNo", accountId);
                        command.Parameters.AddWithValue("@StartDate", frmDate);
                        command.Parameters.AddWithValue("@EndDate", toDate);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read() && !reader.IsDBNull(reader.GetOrdinal("CurrencyImage")))
                            {
                                svgText = reader["CurrencyImage"]?.ToString()?.Trim().TrimStart('\uFEFF');
                            }
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(svgText))
                {
                    SetCurrencyImageNull();
                    return;
                }

                // Convert SVG to Bitmap
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
                    // Failed to convert SVG
                    bitmap = null;
                }

                // Apply the bitmap to your specific picture boxes
                if (FindControl("xrPictureBox2", true) is XRPictureBox pictureBoxDr)
                {
                    pictureBoxDr.Image = bitmap;
                    pictureBoxDr.Sizing = ImageSizeMode.Normal;
                }

                if (FindControl("xrPictureBox3", true) is XRPictureBox pictureBoxCr)
                {
                    pictureBoxCr.Image = bitmap;
                    pictureBoxCr.Sizing = ImageSizeMode.Normal;
                }
                if (FindControl("xrPictureBox4", true) is XRPictureBox pictureBox4)
                {
                    pictureBox4.Image = bitmap;
                    pictureBox4.Sizing = ImageSizeMode.Normal;
                }
                if (FindControl("xrPictureBox5", true) is XRPictureBox pictureBox5)
                {
                    pictureBox5.Image = bitmap;
                    pictureBox5.Sizing = ImageSizeMode.Normal;
                }
                if (FindControl("xrPictureBox6", true) is XRPictureBox pictureBox6)
                {
                    pictureBox6.Image = bitmap;
                    pictureBox6.Sizing = ImageSizeMode.Normal;
                }

            }
            catch
            {
                SetCurrencyImageNull();
            }
        }


        private void SetCurrencyImageNull()
        {
            string[] pictureBoxNames = { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7" };

            foreach (string name in pictureBoxNames)
            {
                if (FindControl(name, true) is XRPictureBox pictureBox)
                {
                    pictureBox.Image = null;
                    pictureBox.ImageSource = null;
                }
            }
        }
    }
}
