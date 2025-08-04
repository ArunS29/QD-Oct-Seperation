using System;
using System.Drawing;
using System.IO;
using System.Text;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Microsoft.Data.SqlClient;
using QD.ERP.Web.Service;
using Svg;

namespace QD.ERP.Web.Areas.Finance.Reports
{
    public partial class BillsReceivableAgeingToday : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public BillsReceivableAgeingToday(
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
            ConfigureDataSource(accountId, toDate);
            LoadCurrencySymbolAndImage();

        }

        public BillsReceivableAgeingToday()
        {
            InitializeComponent();
        }

        private void ConfigureDataSource(string accountId, DateTime toDate)
        {
            ExecuteAgeingStoredProcedure(toDate);

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                throw new Exception("Unable to get tenant context.");

            var connectionParams = new CustomStringConnectionParameters(tenant.ConnectionString);
            sqlDataSource1 = new SqlDataSource(connectionParams);

            var query = new CustomSqlQuery
            {
                Name = "qry20105BillsReceivableAgeingView",
                Sql = @"
                    SELECT * 
                    FROM qry20105BillsReceivableAgeingView 
                    WHERE (@AccountID IS NULL OR AccountHeadNo = @AccountID)
                    AND VoucherDate BETWEEN @StartDate AND @EndDate"
            };

            query.Parameters.AddRange(new[]
            {
                new QueryParameter("@AccountID", typeof(string), accountId ?? ""),
                new QueryParameter("@StartDate", typeof(DateTime), Parameters["StartDate"].Value),
                new QueryParameter("@EndDate", typeof(DateTime), Parameters["EndDate"].Value),
            });

            sqlDataSource1.Queries.Clear();
            sqlDataSource1.Queries.Add(query);
            sqlDataSource1.RebuildResultSchema();
            sqlDataSource1.Fill();

            this.DataSource = sqlDataSource1;
            this.DataMember = "qry20105BillsReceivableAgeingView";
        }

        private void ExecuteAgeingStoredProcedure(DateTime endDate)
        {
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                throw new Exception("Unable to get tenant context for stored procedure.");

            using var connection = new SqlConnection(tenant.ConnectionString);
            connection.Open();

            foreach (var spName in new[] { "sp20124AgeingReports", "sp20125AgeingReceivableReports" })
            {
                using var cmd = new SqlCommand(spName, connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure,
                    CommandTimeout = 120
                };
                cmd.Parameters.AddWithValue("@EndDate", endDate);
                cmd.ExecuteNonQuery();
            }
        }

        private void SetReportParameters(
            string accountId, DateTime frmDate, DateTime toDate,
            string tenantName, string companyName, string companyAddress,
            Image logoImage, string companyNameAr, string companyAddressArb, string username)
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
            AddOrUpdateParameter("StartDate", frmDate, typeof(DateTime));
            AddOrUpdateParameter("EndDate", toDate, typeof(DateTime));
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string));
            AddOrUpdateParameter("UserName", username ?? "", typeof(string));

            // Set static text bindings
            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = companyName;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = companyAddress;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = companyNameAr;

            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = companyAddressArb;

            if (FindControl("xrLabelUserName", true) is XRLabel userLabel)
                userLabel.Text = username;

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox && logoImage != null)
                logoPictureBox.Image = logoImage;
        }

        private void LoadCurrencySymbolAndImage()
        {
            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    SetCurrencyImageNull();
                    return;
                }

                string svgText = null, currencySymbol = null;
                using (var conn = new SqlConnection(tenant.ConnectionString))
                {
                    conn.Open();
                    string sql = $@"
                        SELECT c.CurrencyImage, c.CurrencySymbol
                        FROM {tenant.schemaname}.tbl901CompanyDetails AS c
                        INNER JOIN dbo.fn_GetDefaultCompanyDetails() AS f ON c.CompanyId = f.CompanyId";

                    using var cmd = new SqlCommand(sql, conn);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        svgText = reader["CurrencyImage"]?.ToString()?.Trim('\uFEFF');
                        currencySymbol = reader["CurrencySymbol"]?.ToString()?.Trim();
                    }
                }

                if (FindControl("xrLabelCurrencySymbol", true) is XRLabel label && !string.IsNullOrEmpty(currencySymbol))
                    label.Text = currencySymbol;

                if (string.IsNullOrWhiteSpace(svgText))
                {
                    SetCurrencyImageNull();
                    return;
                }

                Bitmap bitmap = null;
                try
                {
                    using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgText));
                    bitmap = SvgDocument.Open<SvgDocument>(stream).Draw();
                }
                catch { bitmap = null; }

                if (bitmap == null)
                {
                    SetCurrencyImageNull();
                    return;
                }

                string[] pictureBoxes = { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9" };
                foreach (string name in pictureBoxes)
                {
                    if (FindControl(name, true) is XRPictureBox pic)
                    {
                        pic.Image = bitmap;
                        pic.Sizing = ImageSizeMode.Normal;
                    }
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
            string[] pictureBoxNames = { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9" };
            foreach (string name in pictureBoxNames)
            {
                if (FindControl(name, true) is XRPictureBox pictureBox)
                {
                    pictureBox.Image = null;
                    pictureBox.ImageSource = null;
                }
            }

            if (FindControl("xrLabelCurrencySymbol", true) is XRLabel label)
                label.Text = "";
        }

        private void AlignCurrencyWithAmount(Bitmap bitmap, float iconSize = 14f, float padding = 12f)
        {
            var pairs = new[]
            {
                new { Label = "xrLabel5", Picture = "xrPictureBox8" },
                new { Label = "xrLabel7", Picture = "xrPictureBox9" },
                new { Label = "xrLabel4", Picture = "xrPictureBox10" },
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

                    using var g = Graphics.FromImage(new Bitmap(1, 1));
                    using var sysFont = new Font(lbl.Font.Name, lbl.Font.Size, (FontStyle)(int)lbl.Font.Style);

                    float iconHeight = iconSize;
                    float iconWidth = iconSize;

                    pictureBox.WidthF = iconWidth;
                    pictureBox.HeightF = iconHeight;

                    float posY = lbl.LocationF.Y + (lbl.HeightF - iconHeight) / 2f;
                    var format = StringFormat.GenericTypographic;
                    format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

                    float textWidth = g.MeasureString(lbl.Text ?? "", sysFont, int.MaxValue, format).Width;
                    float spaceWidth = g.MeasureString(" ", sysFont).Width;

                    float rightEdge = lbl.LocationF.X + lbl.WidthF;
                    float posX = rightEdge - textWidth - iconWidth - padding - spaceWidth;

                    pictureBox.LocationF = new PointF(posX, posY);
                };
            }
        }
    }
}
