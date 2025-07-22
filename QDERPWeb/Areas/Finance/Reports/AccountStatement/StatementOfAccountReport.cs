using System;
using System.Drawing;
using DevExpress.XtraReports.UI;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;
using QD.ERP.Web.Service;
using DevExpress.XtraPrinting;
using Svg;
using System.Text;

namespace QD.ERP.Web.Reports
{
    public partial class StatementOfAccountReport : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public StatementOfAccountReport(
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

        public StatementOfAccountReport()
        {
            InitializeComponent();
        }

        private void SetReportParameters(
            string accountId,
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressArb,
            string username)
        {
            accountId ??= "";
            frmDate = frmDate == DateTime.MinValue ? DateTime.Today : frmDate;
            toDate = toDate == DateTime.MinValue ? DateTime.Today : toDate;

            AddOrUpdateParameter("AccountID", accountId, typeof(string), false);
            AddOrUpdateParameter("StartDate", frmDate, typeof(DateTime), false);
            AddOrUpdateParameter("EndDate", toDate, typeof(DateTime), false);
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string), false);
            AddOrUpdateParameter("UserName", username ?? "", typeof(string), false);

            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelUserName", true) is XRLabel userLabel)
                userLabel.Text = username;

            if (FindControl("xrLabelCompanyName", true) is XRLabel companyLabel)
                companyLabel.Text = companyName;

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
                if (_tenantDbContextHelper == null || !_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out _))
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

                //if (FindControl("xrPictureBox4", true) is XRPictureBox pictureBox4)
                //{
                //    pictureBox4.Image = bitmap;
                //    pictureBox4.Sizing = ImageSizeMode.Normal;
                //}

                //if (FindControl("xrPictureBox5", true) is XRPictureBox pictureBox5)
                //{
                //    pictureBox5.Image = bitmap;
                //    pictureBox5.Sizing = ImageSizeMode.Normal;
                //}

                //if (FindControl("xrPictureBox6", true) is XRPictureBox pictureBox6)
                //{
                //    pictureBox6.Image = bitmap;
                //    pictureBox6.Sizing = ImageSizeMode.Normal;
                //}

                // NEW: Align bitmap next to currency fields dynamically
                AlignCurrencyWithAmount(bitmap);
            }
            catch
            {
                SetCurrencyImageNull();
            }
        }

        private void SetCurrencyImageNull()
        {
            string[] pictureBoxNames =
            {
        "xrPictureBox4", "xrPictureBox5", "xrPictureBox6",
        "xrPictureBox7", "xrPictureBox8", "xrPictureBox2", "xrPictureBox3"
    };

            foreach (string name in pictureBoxNames)
            {
                if (FindControl(name, true) is XRPictureBox pictureBox)
                {
                    pictureBox.Image = null;
                    pictureBox.ImageSource = null;
                }
            }
        }

        // NEW: This method aligns the currency icon next to the matching labels
        private void AlignCurrencyWithAmount(Bitmap bitmap, float iconSize = 14f, float padding = 12f)
        {
            var fixedPictureBoxes = new[] { "xrPictureBox2", "xrPictureBox3" };
            foreach (var name in fixedPictureBoxes)
            {
                if (FindControl(name, true) is XRPictureBox picBox)
                {
                    picBox.Image = bitmap;
                    picBox.Sizing = ImageSizeMode.StretchImage;
                    picBox.WidthF = iconSize;
                    picBox.HeightF = iconSize;
                }
            }
            var pairs = new[]
   {
    new { Label = "xrLabel7", Picture = "xrPictureBox4" },
    new { Label = "xrLabel8", Picture = "xrPictureBox5" },
    new { Label = "xrLabel11", Picture = "xrPictureBox8" },
                    new { Label = "xrLabel15", Picture = "xrPictureBox6" },

                                    new { Label = "xrLabel18", Picture = "xrPictureBox7" },




};

            foreach (var p in pairs)
            {
                var label = FindControl(p.Label, true) as XRLabel;
                var pictureBox = FindControl(p.Picture, true) as XRPictureBox;

                if (label == null || pictureBox == null)
                    continue;

                pictureBox.Image = bitmap;
                pictureBox.Sizing = ImageSizeMode.StretchImage;

                label.BeforePrint += (s, e) =>
                {
                    var lbl = (XRLabel)s;

                    using (var g = Graphics.FromImage(new Bitmap(1, 1)))
                    using (var sysFont = new Font(lbl.Font.Name, lbl.Font.Size, (FontStyle)(int)lbl.Font.Style))
                    {
                        float iconHeight = lbl.Font.Size + 0.2f;// Match icon to font height
                        float iconWidth = iconHeight;            // Keep square

                        pictureBox.WidthF = iconWidth;
                        pictureBox.HeightF = iconHeight;

                        float posY = lbl.LocationF.Y + (lbl.HeightF - iconHeight) / 2f;

                        var format = StringFormat.GenericTypographic;
                        format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

                        float textWidth = g.MeasureString(lbl.Text ?? "", sysFont, int.MaxValue, format).Width;
                        float spaceWidth = g.MeasureString(" ", sysFont).Width;

                        float rightEdge = lbl.LocationF.X + lbl.WidthF;
                        float posX = rightEdge - textWidth - iconWidth - 5f - spaceWidth;

                        pictureBox.LocationF = new PointF(posX, posY);
                    }
                };
            }
        }
    }
}
