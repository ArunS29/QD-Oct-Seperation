using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Microsoft.Data.SqlClient;
using Svg;
using System.Drawing;
using System.Text;

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
            LoadCurrencySymbolAndImage();

            sqlDataSource1 = new SqlDataSource();
            this.DataSource = sqlDataSource1;

            SetReportParameters(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb, username);

            try
            {
                sqlDataSource1.Fill();
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data: " + ex.Message, ex);
            }
        }

        public BillsReceivableAgeingToday()
        {
            InitializeComponent();
        }

        private void SetReportParameters(string accountId, DateTime frmDate,
     DateTime toDate, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressArb, string username)
        {
            var today = DateTime.Today;

            AddOrUpdateParameter("AccountID", accountId ?? "", typeof(string), false);
            AddOrUpdateParameter("StartDate", frmDate, typeof(DateTime), false);
            AddOrUpdateParameter("EndDate", toDate, typeof(DateTime), false);
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string), false);
            AddOrUpdateParameter("UserName", username ?? "", typeof(string), false);

            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
            {
                tenantLabel.ExpressionBindings.Clear();
                tenantLabel.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "?TenantName"));
            }

            if (FindControl("xrLabelUserName", true) is XRLabel userNameLabel)
            {
                userNameLabel.ExpressionBindings.Clear();
                userNameLabel.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "?UserName"));
            }

            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
            {
                companyNameLabel.ExpressionBindings.Clear();
                companyNameLabel.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "?CompanyName"));
            }

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
            {
                addressLabel.ExpressionBindings.Clear();
                addressLabel.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "?CompanyAddress"));
            }

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
            {
                companyNameArLabel.ExpressionBindings.Clear();
                companyNameArLabel.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "?CompanyNameAr"));
            }

            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
            {
                addressArbLabel.ExpressionBindings.Clear();
                addressArbLabel.ExpressionBindings.Add(new ExpressionBinding("BeforePrint", "Text", "?CompanyAddressArb"));
            }

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
            if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                var connectionParams = new CustomStringConnectionParameters(tenant.ConnectionString);
                sqlDataSource1 = new SqlDataSource(connectionParams);

                var querySql = @"
            SELECT * 
            FROM qry20105BillsReceivableAgeingView 
            WHERE (@AccountID IS NULL OR AccountHeadNo = @AccountID)
            AND VoucherDate BETWEEN @StartDate AND @EndDate";

                var customQuery = new CustomSqlQuery
                {
                    Name = "qry20105BillsReceivableAgeingView",
                    Sql = querySql
                };

                customQuery.Parameters.AddRange(new[]
                {
            new QueryParameter("@AccountID", typeof(string), accountId ?? ""),
            new QueryParameter("@StartDate", typeof(DateTime), frmDate),
            new QueryParameter("@EndDate", typeof(DateTime), toDate)
        });

                sqlDataSource1.Queries.Clear();
                sqlDataSource1.Queries.Add(customQuery);
                sqlDataSource1.Name = "sqlDataSource1";

                sqlDataSource1.RebuildResultSchema();

                this.DataSource = sqlDataSource1;
                this.DataMember = "qry20105BillsReceivableAgeingView";
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

                // Step 3: Modify SVG to reduce boldness
                svgText = svgText
     .Replace("font-weight:bold", "font-weight:normal")
     .Replace("font-weight:700", "font-weight:200")
     .Replace("font-weight:600", "font-weight:200")
     .Replace("font-weight:500", "font-weight:200")
     .Replace("font-weight:800", "font-weight:200")
     .Replace("stroke-width:2", "stroke-width:0.2")
     .Replace("stroke-width:1.5", "stroke-width:0.2")
     .Replace("stroke-width:1", "stroke-width:0.2")
     .Replace("stroke:#000", "stroke:#666") // ← slightly lighter stroke instead of "none"
     .Replace("stroke:black", "stroke:#666") // ← same here
     .Replace("stroke:gray", "stroke:#999")
     .Replace("fill:#000000", "fill:#444444") // ← dark gray fill instead of full black
     .Replace("fill:black", "fill:#444444")
     .Replace("fill-opacity=\"1\"", "fill-opacity=\"0.7\""); // slightly transparent

                if (!svgText.Contains("font-weight"))
                {
                    svgText = svgText.Replace("<text", "<text style=\"font-weight:200\"");
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

                 string[] pictureBoxNames = { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7" };

                foreach (string name in pictureBoxNames)
                 {
                    if (FindControl(name, true) is XRPictureBox pictureBox)
                      {
                      pictureBox.Image = bitmap;
                       pictureBox.Sizing = ImageSizeMode.Normal;
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
            string[] pictureBoxNames = { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7" };

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

        //        private void AlignCurrencyWithAmount(Bitmap bitmap)
        //        {
        //            var pairs = new[]
        //            {
        //           new { Label = "xrLabel43", Picture = "xrPictureBox2" },
        //            new { Label = "xrLabel44", Picture = "xrPictureBox3" },
        //            new { Label = "xrLabel45", Picture = "xrPictureBox4" },
        //            new { Label = "xrLabel5", Picture = "xrPictureBox5" },
        //            new { Label = "xrLabel7", Picture = "xrPictureBox7" },
        //            new { Label = "xrLabel4", Picture = "xrPictureBox6" },



        //};

        //            foreach (var p in pairs)
        //            {
        //                var label = FindControl(p.Label, true) as XRLabel;
        //                var pictureBox = FindControl(p.Picture, true) as XRPictureBox;

        //                if (label == null || pictureBox == null)
        //                    continue;

        //                pictureBox.Image = bitmap;
        //                pictureBox.Sizing = ImageSizeMode.StretchImage;

        //                label.BeforePrint += (s, e) =>
        //                {
        //                    var lbl = (XRLabel)s;

        //                    float iconWidth = 10f;
        //                    float iconHeight = 10f;

        //                    pictureBox.WidthF = iconWidth;
        //                    pictureBox.HeightF = iconHeight;

        //                    // Center the icon vertically with respect to the label
        //                    float posY = lbl.LocationF.Y + (lbl.HeightF - iconHeight) / 2f;

        //                    // Convert DXFont to System.Drawing.Font manually
        //                    using (var g = Graphics.FromImage(new Bitmap(1, 1)))
        //                    {
        //                        using (var sysFont = new Font(lbl.Font.Name, lbl.Font.Size, (FontStyle)(int)lbl.Font.Style))
        //                        {
        //                            var format = StringFormat.GenericTypographic;
        //                            format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

        //                            float textWidth = g.MeasureString(lbl.Text ?? "", sysFont, int.MaxValue, format).Width;

        //                            // Align image to left of text with 5 units padding
        //                            float rightEdge = lbl.LocationF.X + lbl.WidthF;
        //                            float posX = rightEdge - textWidth - iconWidth - 8f; // Adjusted spacing for visual gap

        //                            pictureBox.LocationF = new PointF(posX, posY);
        //                        }
        //                    }
        //                };
        //            }
        //        }


        //    private void AlignCurrencyWithAmount(Bitmap bitmap)
        //    {
        //        var pairs = new[]
        //        {


        //    new { Label = "xrLabel5",  Picture = "xrPictureBox5" },
        //    new { Label = "xrLabel7",  Picture = "xrPictureBox7" },
        //    new { Label = "xrLabel4",  Picture = "xrPictureBox6" },
        //};

        //        foreach (var p in pairs)
        //        {
        //            var label = FindControl(p.Label, true) as XRLabel;
        //            var pictureBox = FindControl(p.Picture, true) as XRPictureBox;

        //            if (label == null || pictureBox == null)
        //                continue;

        //            pictureBox.Image = bitmap;
        //            pictureBox.Sizing = ImageSizeMode.StretchImage;

        //            label.BeforePrint += (s, e) =>
        //            {
        //                var lbl = (XRLabel)s;

        //                using (var g = Graphics.FromImage(new Bitmap(1, 1)))
        //                using (var sysFont = new Font(lbl.Font.Name, lbl.Font.Size, (FontStyle)(int)lbl.Font.Style))
        //                {
        //                    float iconHeight = lbl.Font.Size + 0.2f;// Match icon to font height
        //                    float iconWidth = iconHeight;            // Keep square

        //                    pictureBox.WidthF = iconWidth;
        //                    pictureBox.HeightF = iconHeight;

        //                    float posY = lbl.LocationF.Y + (lbl.HeightF - iconHeight) / 2f;

        //                    var format = StringFormat.GenericTypographic;
        //                    format.FormatFlags |= StringFormatFlags.MeasureTrailingSpaces;

        //                    float textWidth = g.MeasureString(lbl.Text ?? "", sysFont, int.MaxValue, format).Width;
        //                    float spaceWidth = g.MeasureString(" ", sysFont).Width;

        //                    float rightEdge = lbl.LocationF.X + lbl.WidthF;
        //                    float posX = rightEdge - textWidth - iconWidth - 5f - spaceWidth;

        //                    pictureBox.LocationF = new PointF(posX, posY);
        //                }
        //            };
        //        }
        //    }



        private void AlignCurrencyWithAmount(Bitmap bitmap, float iconSize = 14f, float padding = 12f)
        {
            var fixedPictureBoxes = new[] { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4" };
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
        new { Label = "xrLabel5",  Picture = "xrPictureBox8" },
        new { Label = "xrLabel7",  Picture = "xrPictureBox9" },
        new { Label = "xrLabel4",  Picture = "xrPictureBox10" },
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
                    }
                };
            }
        }


    }
}
