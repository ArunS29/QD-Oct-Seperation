using System;
using System.Drawing;
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
    public partial class BillsReceivableAll : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public BillsReceivableAll(
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
            LoadCurrencySymbolAndImage();

            try
            {
                sqlDataSource1.Fill();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data: " + ex.Message, ex);
            }
        }

        public BillsReceivableAll()
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
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string), false);
            AddOrUpdateParameter("UserName", username ?? "", typeof(string), false);

            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelUserName", true) is XRLabel userNameLabel)
                userNameLabel.Text = username;

            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = companyName;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = companyAddress;

            if (FindControl("xrPictureBox3", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = companyNameAr;

            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = companyAddressArb;

            ConfigureDataSource(accountId, frmDate, toDate);
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

        private void ConfigureDataSource(string accountId, DateTime frmDate, DateTime toDate)
        {
            sqlDataSource1.Queries.Clear();

            if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                sqlDataSource1.ConnectionParameters = new CustomStringConnectionParameters(tenant.ConnectionString);

                string querySql = @"SELECT * FROM qry201SubLedgerReceivablesMaster 
                                WHERE (@AccountID IS NULL OR AccountHeadNo = @AccountID)
                                AND VoucherDate BETWEEN @StartDate AND @EndDate";

                var customQuery = new CustomSqlQuery
                {
                    Name = "qry201SubLedgerReceivablesMaster",
                    Sql = querySql
                };

                customQuery.Parameters.AddRange(new[]
                {
                new QueryParameter("@AccountID", typeof(string), accountId),
                new QueryParameter("@StartDate", typeof(DateTime), frmDate),
                new QueryParameter("@EndDate", typeof(DateTime), toDate)
            });

                sqlDataSource1.Queries.Add(customQuery);
                sqlDataSource1.Name = "sqlDataSource1";
            }
            else
            {
                throw new Exception("Unable to get tenant context. Please check session and cache.");
            }
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

                    string sql = $@"
                        SELECT c.CurrencyImage, c.CurrencySymbol
                        FROM {tenant.schemaname}.tbl901CompanyDetails AS c
                        INNER JOIN dbo.fn_GetDefaultCompanyDetails() AS f
                            ON c.CompanyId = f.CompanyId";

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

                        // 🔽 Reduce boldness by lowering stroke width
                        foreach (var element in svgDoc.Descendants().OfType<SvgVisualElement>())
                        {
                            if (element.StrokeWidth > 0)
                                element.StrokeWidth = 0.3f;

                            if (element.Stroke == null)
                                element.Stroke = new SvgColourServer(Color.Black);

                            if (element.Fill == null)
                                element.Fill = new SvgColourServer(Color.Black);
                        }

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

                string[] pictureBoxNames = { "xrPictureBox2", "xrPictureBox1", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9", "xrPictureBox10", "xrPictureBox11", "xrPictureBox12" };

                foreach (string name in pictureBoxNames)
                {
                    if (FindControl(name, true) is XRPictureBox pictureBox)
                    {
                        pictureBox.Image = bitmap;
                        pictureBox.Sizing = ImageSizeMode.Squeeze;
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
            string[] pictureBoxNames = { "xrPictureBox2", "xrPictureBox1", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9" ,"xrPictureBox10", "xrPictureBox11", "xrPictureBox12" };

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


        private void AlignCurrencyWithAmount(Bitmap bitmap)
        {
            var pairs = new[]
            {
        new { Label = "xrLabel52", Picture = "xrPictureBox5" },
        new { Label = "xrLabel53", Picture = "xrPictureBox6" },
        new { Label = "xrLabel54", Picture = "xrPictureBox7" },
                //new { Label = "xrLabel14", Picture = "xrPictureBox5" },
                //new { Label = "xrLabel9", Picture = "xrPictureBox4" },
                //new { Label = "xrLabel6", Picture = "xrPictureBox11" },
                //new { Label = "xrLabel8", Picture = "xrPictureBox12" },


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

    }
}
