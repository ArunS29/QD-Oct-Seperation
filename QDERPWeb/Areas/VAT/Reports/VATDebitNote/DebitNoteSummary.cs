using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Svg;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;

namespace QD.ERP.Web.Areas.VAT.Reports.VATDebitNote
{
    public partial class DebitNoteSummary : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        public DebitNoteSummary()
        {
            InitializeComponent();
        }

        public DebitNoteSummary(
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressAr,
            string username,
            TenantDbContextHelper tenantDbContextHelper,Decimal DefaultCurrencyDecimals)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            InitializeComponent();
            SetReportParameters(frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr,username, DefaultCurrencyDecimals);
            LoadCurrencySymbolAndImage();
        }

        private void SetReportParameters(
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressAr,
            string username,Decimal DefaultCurrencyDecimals)
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

            AddOrUpdateParameter("StartDate", frmDate, typeof(DateTime));
            AddOrUpdateParameter("EndDate", toDate, typeof(DateTime));
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressAr", companyAddressAr ?? "", typeof(string));
            AddOrUpdateParameter("UserName", username ?? "", typeof(string));

            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = companyName;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = companyAddress;

            if (FindControl("xrPictureBox2", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = companyNameAr;

            if (FindControl("xrLabelCompanyAddressAr", true) is XRLabel addressArLabel)
                addressArLabel.Text = companyAddressAr;
            if (FindControl("xrLabelUaserName", true) is XRLabel usernameLabel)
                usernameLabel.Text = companyAddressAr;
            // ✅ Change decimal formatting only if value is 3
            if (DefaultCurrencyDecimals == 3)
            {
                if (FindControl("xrTableCell27", true) is XRLabel lbl27)
                    lbl27.TextFormatString = "{0:n3}";
                if (FindControl("xrTableCell26", true) is XRLabel lbl26)
                    lbl26.TextFormatString = "{0:n3}";
                if (FindControl("xrTableCell25", true) is XRLabel lbl25)
                    lbl25.TextFormatString = "{0:n3}";
                if (FindControl("xrTableCell26", true) is XRLabel lbl32)
                    lbl32.TextFormatString = "{0:n3}";
                if (FindControl("xrTableCell33", true) is XRLabel lbl33)
                    lbl33.TextFormatString = "{0:n3}";
                if (FindControl("xrTableCell24", true) is XRLabel lbl24)
                    lbl24.TextFormatString = "{0:n3}";
                if (FindControl("xrTableCell31", true) is XRLabel lbl31)
                    lbl31.TextFormatString = "{0:n3}";
                if (FindControl("xrTableCell30", true) is XRLabel lbl30)
                    lbl30.TextFormatString = "{0:n3}";
                if (FindControl("xrTableCell29", true) is XRLabel lbl29)
                    lbl29.TextFormatString = "{0:n3}";
                if (FindControl("xrTableCell34", true) is XRLabel lbl34)
                    lbl34.TextFormatString = "{0:n3}";
                if (FindControl("xrTableCell26", true) is XRLabel lbl35)
                    lbl35.TextFormatString = "{0:n3}";
                if (FindControl("tableCell32", true) is XRTableCell cell32)
                    cell32.TextFormatString = "{0:n3}";
                if (FindControl("tableCell34", true) is XRTableCell cell34)
                    cell34.TextFormatString = "{0:n3}";
                if (FindControl("tableCell35", true) is XRTableCell cell35)
                    cell35.TextFormatString = "{0:n3}";
                if (FindControl("tableCell27", true) is XRTableCell cell27)
                    cell27.TextFormatString = "{0:n3}";

                if (FindControl("xrLabel2", true) is XRLabel lbl2)
                    lbl2.TextFormatString = "{0:n3}";
                if (FindControl("xrLabel3", true) is XRLabel lbl3)
                    lbl3.TextFormatString = "{0:n3}";
                if (FindControl("xrLabel4", true) is XRLabel lbl4)
                    lbl4.TextFormatString = "{0:n3}";
                if (FindControl("xrLabel5", true) is XRLabel lbl5)
                    lbl5.TextFormatString = "{0:n3}";
                if (FindControl("xrLabel6", true) is XRLabel lbl6)
                    lbl6.TextFormatString = "{0:n3}";
                if (FindControl("xrLabel7", true) is XRLabel lbl7)
                    lbl7.TextFormatString = "{0:n3}";
                if (FindControl("xrLabel8", true) is XRLabel lbl8)
                    lbl8.TextFormatString = "{0:n3}";
                if (FindControl("xrLabel9", true) is XRLabel lbl9)
                    lbl9.TextFormatString = "{0:n3}";
                if (FindControl("xrLabel14", true) is XRLabel lbl14)
                    lbl14.TextFormatString = "{0:n3}";
                if (FindControl("xrLabel15", true) is XRLabel lbl15)
                    lbl15.TextFormatString = "{0:n3}";
                if (FindControl("xrLabel16", true) is XRLabel lbl16)
                    lbl16.TextFormatString = "{0:n3}";
                if (FindControl("xrLabel20", true) is XRLabel lbl20)
                    lbl20.TextFormatString = "{0:n3}";
                if (FindControl("xrLabel21", true) is XRLabel lbl21)
                    lbl21.TextFormatString = "{0:n3}";
                if (FindControl("xrLabel22", true) is XRLabel lbl22)
                    lbl22.TextFormatString = "{0:n3}";
                if (FindControl("xrLabel23", true) is XRLabel lbl23)
                    lbl23.TextFormatString = "{0:n3}";
                if (FindControl("xrLabel18", true) is XRLabel lbl18)
                    lbl18.TextFormatString = "{0:n3}";
                if (FindControl("xrLabel19", true) is XRLabel lbl19)
                    lbl19.TextFormatString = "{0:n3}";
            

        }
            ConfigureSqlDataSource(frmDate, toDate);
        }

        private void ConfigureSqlDataSource(DateTime frmDate, DateTime toDate)
        {
            var selectQuery = new CustomSqlQuery()
            {
                Name = "qry201_907VATDebitNoteRegisterMainView",
                Sql = @"SELECT * FROM  qry201_907VATDebitNoteRegisterMainView
                        WHERE DebitNoteDate BETWEEN @StartDate AND @EndDate"
            };

            selectQuery.Parameters.AddRange(new[]
            {
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

            this.sqlDataSource1.Queries.Clear();
            this.sqlDataSource1.Queries.Add(selectQuery);

            if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                var connectionParams = new CustomStringConnectionParameters(tenant.ConnectionString);
                this.sqlDataSource1.ConnectionParameters = connectionParams;
            }
            else
            {
                throw new Exception("Unable to get tenant context. Please check session and cache.");
            }

            this.sqlDataSource1.Fill();
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

                string[] pictureBoxNames = { "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9", "xrPictureBox10" };

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
            string[] pictureBoxNames = { "xrPictureBox1", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9", "xrPictureBox10", "xrPictureBox4" };

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

        private void AlignCurrencyWithAmount(Bitmap bitmap, float iconSize = 14f, float padding = 12f)
        {
            var fixedPictureBoxes = new[] { "xrPictureBox1", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9", "xrPictureBox10",  "xrPictureBox12", "xrPictureBox13", "xrPictureBox14", "xrPictureBox15", "xrPictureBox16", "xrPictureBox17", "xrPictureBox18", "xrPictureBox19", "xrPictureBox20", "xrPictureBox11" };
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


    //new { Label = "xrLabel7",  Picture = "xrPictureBox3" },
    //new { Label = "xrLabel5",  Picture = "xrPictureBox2" },
    //new { Label = "xrLabel6",  Picture = "xrPictureBox4" },
    new { Label = "xrLabel0",  Picture = "xrPictureBox0" },
    //new { Label = "xrLabel10",  Picture = "xrPictureBox3" },
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
                        float customPadding = 0f;




                        float posX = rightEdge - textWidth - iconWidth - 15f - spaceWidth;


                        pictureBox.LocationF = new PointF(posX, posY);
                    }
                };
            }
        }
    }
}
