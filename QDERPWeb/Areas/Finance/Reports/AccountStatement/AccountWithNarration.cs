using System;
using System.Drawing;
using System.IO;
using System.Text;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraPrinting;
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

            SetReportParameters(accountId, frmDate, toDate, tenantName, companyName,
                                companyAddress, logoImage, companyNameAr,
                                companyAddressArb, username);

            try
            {
                sqlDataSource2.Fill();          // ⬅️ existing data fill
                LoadCurrencyImage(accountId, frmDate, toDate);   // ⬅️ now positions image dynamically
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data: " + ex.Message, ex);
            }
        }

        public AccountWithNarration() => InitializeComponent();

        /* ------------------------------------------------------------------ */
        /* ---------------------  PARAMETER / DATASOURCE  ------------------- */
        /* ------------------------------------------------------------------ */

        private void SetReportParameters(string accountId,
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
            AddOrUpdateParameter("UserName", username ?? "", typeof(string), false);
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string), false);

            (FindControl("xrLabelUserName", true) as XRLabel)?.SetText(username);
            (FindControl("xrLabelTenantName", true) as XRLabel)?.SetText(tenantName);
            (FindControl("xrLabelCompanyName", true) as XRLabel)?.SetText(companyName);
            (FindControl("xrLabelCompanyAddress", true) as XRLabel)?.SetText(companyAddress);
            (FindControl("xrPictureBox1", true) as XRPictureBox)?.SetImage(logoImage);
            (FindControl("xrLabelCompanyNameAr", true) as XRLabel)?.SetText(companyNameAr);
            (FindControl("xrLabelCompanyAddressArb", true) as XRLabel)?.SetText(companyAddressArb);

            ConfigureSqlDataSource(accountId, frmDate, toDate);
        }

        private void AddOrUpdateParameter(string paramName, object paramValue,
                                          Type paramType, bool visible)
        {
            var p = Parameters[paramName];
            if (p == null)
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
                p.Value = paramValue;
                p.Visible = visible;
            }
        }

        private void ConfigureSqlDataSource(string accountId, DateTime frmDate, DateTime toDate)
        {
            sqlDataSource2.Queries.Clear();

            if (_tenantDbContextHelper != null &&
                _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out _))
            {
                sqlDataSource2.ConnectionParameters =
                    new CustomStringConnectionParameters(tenant.ConnectionString);

                string schema = string.IsNullOrWhiteSpace(tenant.schemaname) ? "dbo" : tenant.schemaname;
                string procName = $"{schema}.StProAccountLedger";

                var q = new StoredProcQuery
                {
                    Name = "StProAccountLedger",
                    StoredProcName = procName
                };
                q.Parameters.AddRange(new[]
                {
                    new QueryParameter("@ParamAccountNo", typeof(string),  accountId),
                    new QueryParameter("@StartDate",      typeof(DateTime),frmDate),
                    new QueryParameter("@EndDate",        typeof(DateTime),toDate)
                });

                sqlDataSource2.Queries.Add(q);
                sqlDataSource2.Name = "sqlDataSource2";
            }
            else
            {
                throw new Exception("Unable to get tenant context. Please check session / cache.");
            }
        }

        /* ------------------------------------------------------------------ */
        /* ---------------------   CURRENCY  SVG / BITMAP   ------------------ */
        /* ------------------------------------------------------------------ */

        private void LoadCurrencyImage(string accountId, DateTime frmDate, DateTime toDate)
        {
            if (string.IsNullOrEmpty(accountId))
            {
                SetCurrencyImageNull();
                return;
            }

            try
            {
                if (_tenantDbContextHelper == null ||
                    !_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out _))
                {
                    SetCurrencyImageNull();
                    return;
                }

                string svgText = null;
                using (var conn = new System.Data.SqlClient.SqlConnection(tenant.ConnectionString))
                {
                    conn.Open();
                    using var cmd = new System.Data.SqlClient.SqlCommand(
                                        $"{tenant.schemaname}.StProAccountLedger", conn)
                    { CommandType = System.Data.CommandType.StoredProcedure };

                    cmd.Parameters.AddWithValue("@ParamAccountNo", accountId);
                    cmd.Parameters.AddWithValue("@StartDate", frmDate);
                    cmd.Parameters.AddWithValue("@EndDate", toDate);

                    using var r = cmd.ExecuteReader();
                    if (r.Read() && !r.IsDBNull(r.GetOrdinal("CurrencyImage")))
                        svgText = r["CurrencyImage"]?.ToString()?.Trim().TrimStart('\uFEFF');
                }

                if (string.IsNullOrWhiteSpace(svgText))
                {
                    SetCurrencyImageNull();
                    return;
                }

                Bitmap bmp;
                try
                {
                    using var ms = new MemoryStream(Encoding.UTF8.GetBytes(svgText));
                    bmp = SvgDocument.Open<SvgDocument>(ms).Draw();
                }
                catch
                {
                    SetCurrencyImageNull();
                    return;
                }

                AlignCurrencyWithAmount(bmp);
            }
            catch
            {
                SetCurrencyImageNull();
            }
        }

        /* ------------------------------------------------------------------ */
        /* ------------------   IMAGE–AMOUNT DYNAMIC LAYOUT  ----------------- */
        /* ------------------------------------------------------------------ */

        /// <summary>
        /// Ensures the currency bitmap is always flush against the amount’s visible text
        /// (no matter how many digits).  The image keeps a square shape equal to the
        /// label’s height, then its X is shifted so the spacing stays “1 pt” before the
        /// number’s first digit.
        /// </summary>
        private void AlignCurrencyWithAmount(Bitmap bitmap)
        {
            var pairs = new[]
            {
        new { Label = "xrLabel22", Picture = "xrPictureBox2" },
        new { Label = "xrLabel23", Picture = "xrPictureBox3" },
        new { Label = "xrLabel7", Picture = "xrPictureBox6" },
                new { Label = "xrLabel8", Picture = "xrPictureBox5" },
                        new { Label = "xrLabel11", Picture = "xrPictureBox4" },
                        new { Label = "xrLabel5", Picture = "xrPictureBox7" },
                        new { Label = "xrLabel9", Picture = "xrPictureBox8" },

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

                    float iconWidth = 10f;
                    float iconHeight = 10f;

                    pictureBox.WidthF = iconWidth;
                    pictureBox.HeightF = iconHeight;

                    // Center the icon vertically with respect to the label
                    float posY = lbl.LocationF.Y + (lbl.HeightF - iconHeight) / 2f;

                    // Convert DXFont to System.Drawing.Font manually
                    using (var g = Graphics.FromImage(new Bitmap(1, 1)))
                    {
                        using (var sysFont = new Font(lbl.Font.Name, lbl.Font.Size, (FontStyle)(int)lbl.Font.Style))
                        {
                            var format = StringFormat.GenericTypographic;
                            format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

                            float textWidth = g.MeasureString(lbl.Text ?? "", sysFont, int.MaxValue, format).Width;

                            // Align image to left of text with 5 units padding
                            float rightEdge = lbl.LocationF.X + lbl.WidthF;
                            float posX = rightEdge - textWidth - iconWidth - 8f; // Adjusted spacing for visual gap

                            pictureBox.LocationF = new PointF(posX, posY);
                        }
                    }
                };
            }
        }




        private void SetCurrencyImageNull()
        {
            string[] pics = { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4",
                              "xrPictureBox5", "xrPictureBox6", "xrPictureBox7" };

            foreach (var n in pics)
                if (FindControl(n, true) is XRPictureBox pb)
                {
                    pb.Image = null;
                    pb.ImageSource = null;
                }
        }
    }

    internal static class XtraReportExt
    {
        public static void SetText(this XRLabel lbl, string txt) => lbl.Text = txt ?? "";
        public static void SetImage(this XRPictureBox pic, Image img) => pic.Image = img;
    }
}
