using DevExpress.CodeParser; // For TenantDbContextHelper
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using Microsoft.Data.SqlClient;
using QD.ERP.Shared.Service; // Make sure this namespace is included
using Svg;
using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
namespace QD.ERP.Finance.Areas.Finance.Reports.ImportReports.PayableandReceivable
{
    public partial class BillsRecivableReport1Import : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public BillsRecivableReport1Import(
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressArb,
            TenantDbContextHelper tenantDbContextHelper, string username)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            InitializeComponent();
            SetReportParameters(tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb, username);
            LoadCurrencySymbolAndImage();
        }

        public BillsRecivableReport1Import()
        {
            InitializeComponent();
            SetReportParameters("", "", "", null, "", "", "");
        }

        private void SetReportParameters(string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressArb, string username)
        {
            void AddOrUpdateParameter(string name, object value, Type type, bool visible = false)
            {
                if (Parameters[name] == null)
                {
                    Parameters.Add(new Parameter()
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

            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string));
            AddOrUpdateParameter(Name = "Username", value: username ?? "", type: typeof(string), visible: false);
            SetLabelText("xrLabelUsername", username ?? "");
            SetLabelText("xrLabelTenantName", tenantName);
            SetLabelText("xrLabelCompanyName", companyName);
            SetLabelText("xrLabelCompanyAddress", companyAddress);
            SetLabelText("xrLabelCompanyNameAr", companyNameAr);
            SetLabelText("xrLabelCompanyAddressArb", companyAddressArb);

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
            {
                logoPictureBox.Image = logoImage;
                logoPictureBox.Visible = logoImage != null;
            }

            LoadReportData(); // load full static data
        }

        private void SetLabelText(string controlName, string text)
        {
            if (FindControl(controlName, true) is XRLabel label)
            {
                label.Text = text ?? "";
            }
        }

        private void LoadReportData()
        {
            try
            {
                if (this.sqlDataSource2 == null)
                    this.sqlDataSource2 = new SqlDataSource();

                this.sqlDataSource2.Queries.Clear();

                var selectQuery = new CustomSqlQuery
                {
                    Name = "tbl201SubLedgerReceivablesMaster",
                    Sql = "SELECT * FROM tbl201SubLedgerReceivablesMaster"
                };

                this.sqlDataSource2.Queries.Add(selectQuery);

                if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    var connectionParams = new CustomStringConnectionParameters(tenant.ConnectionString);
                    this.sqlDataSource2.ConnectionParameters = connectionParams;
                }
                else
                {
                    throw new Exception("Unable to get tenant context. Please check session and cache.");
                }

                this.sqlDataSource2.RebuildResultSchema();
                this.sqlDataSource2.Fill();

                this.DataSource = sqlDataSource2;
                this.DataMember = selectQuery.Name;

                Console.WriteLine("Static Report Data Loaded Successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Loading Static Report Data: {ex.Message}");
            }
        }

        protected override void OnDataSourceDemanded(EventArgs e)
        {
            base.OnDataSourceDemanded(e);
            Console.WriteLine("Static BillsReceivable Report DataSource Demanded.");
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

                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = $@"
                        SELECT c.CurrencyImage, c.CurrencySymbol
                        FROM {tenant.schemaname}.tbl901CompanyDetails AS c
                        INNER JOIN dbo.fn_GetDefaultCompanyDetails() AS f
                            ON c.CompanyId = f.CompanyId";

                    using (var command = new System.Data.SqlClient.SqlCommand(sql, connection))
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

                string[] pictureBoxNames = { "xrPictureBox2", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9", "xrPictureBox10", "xrPictureBox11", "xrPictureBox12" };

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
            string[] pictureBoxNames = { "xrPictureBox2", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9", "xrPictureBox10", "xrPictureBox11", "xrPictureBox12" };

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
            var fixedPictureBoxes = new[] { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", };
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
    //new { Label = "xrLabel5",  Picture = "xrPictureBox8" },
    //new { Label = "xrLabel7",  Picture = "xrPictureBox9" },
    new { Label = "xrLabel0",  Picture = "xrPictureBox0" },
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
