using System;
using System.Drawing;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.XtraReports.UI;
using QD.ERP.Web.Service;
using DevExpress.XtraPrinting;
using Svg;
using System.Text;

namespace QD.ERP.Web.Areas.Finance.Reports.AccountRegister
{
    public partial class Register4line : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        // Updated constructor to accept TenantDbContextHelper for multitenancy
        public Register4line(
            string voucherType,
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressArb,
            TenantDbContextHelper tenantDbContextHelper,string username)
        {
            _tenantDbContextHelper = tenantDbContextHelper;

            InitializeComponent();
            SetReportParameters(voucherType, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb, username);

            try
            {
                sqlDataSource2.Fill();
                LoadCurrencyImage(voucherType, frmDate, toDate);

            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data: " + ex.Message, ex);
            }
        }

        public Register4line()
        {
            InitializeComponent();
        }

        private void SetReportParameters(string voucherType, DateTime frmDate, DateTime toDate, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressArb,string username)
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
            AddOrUpdateParameter("UserName", username ?? "", typeof(string), false);


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
            sqlDataSource2.Queries.Clear();

            // Multi-tenant connection string setup
            if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                sqlDataSource2.ConnectionParameters = new CustomStringConnectionParameters(tenant.ConnectionString);

                // Use schema from tenant, or default to dbo
                string schemaName = string.IsNullOrWhiteSpace(tenant.schemaname) ? "dbo" : tenant.schemaname;
                string fullStoredProcName = $"{schemaName}.StProAccountLedgerByVoucherType";

                var storedProcQuery = new StoredProcQuery
                {
                    Name = "StProAccountLedgerByVoucherType",
                    StoredProcName = fullStoredProcName
                };

                storedProcQuery.Parameters.AddRange(new[]
                {
            new QueryParameter("@VoucherType", typeof(string), voucherType),
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
        private void LoadCurrencyImage(string voucherType, DateTime frmDate, DateTime toDate)
        {
            if (string.IsNullOrEmpty(voucherType))
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
                    using (var command = new System.Data.SqlClient.SqlCommand($"{tenant.schemaname}.StProAccountLedgerByVoucherType", connection))
                    {
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@VoucherType", voucherType);
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
                if (FindControl("xrPictureBox7", true) is XRPictureBox pictureBox7)
                {
                    pictureBox7.Image = bitmap;
                    pictureBox7.Sizing = ImageSizeMode.Normal;
                }
                if (FindControl("xrPictureBox8", true) is XRPictureBox pictureBox8)
                {
                    pictureBox8.Image = bitmap;
                    pictureBox8.Sizing = ImageSizeMode.Normal;
                }
                if (FindControl("xrPictureBox9", true) is XRPictureBox pictureBox9)
                {
                    pictureBox9.Image = bitmap;
                    pictureBox9.Sizing = ImageSizeMode.Normal;
                }
                AlignCurrencyWithAmount(bitmap);
            }
            catch
            {
                SetCurrencyImageNull();
            }
        }


        private void SetCurrencyImageNull()
        {
            string[] pictureBoxNames = { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox10", "xrPictureBox11", "xrPictureBox8", "xrPictureBox9" };

            foreach (string name in pictureBoxNames)
            {
                if (FindControl(name, true) is XRPictureBox pictureBox)
                {
                    pictureBox.Image = null;
                    pictureBox.ImageSource = null;
                }
            }
        }
        private void AlignCurrencyWithAmount(Bitmap bitmap, float iconSize = 14f, float padding = 12f)
        {
            var fixedPictureBoxes = new[] { "xrPictureBox10", "xrPictureBox11" };
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


    new { Label = "xrLabel21",  Picture = "xrPictureBox6" },
    new { Label = "xrLabel22",  Picture = "xrPictureBox7" },
    new { Label = "xrLabel11",  Picture = "xrPictureBox9" },
    new { Label = "xrLabel12",  Picture = "xrPictureBox8" },
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
