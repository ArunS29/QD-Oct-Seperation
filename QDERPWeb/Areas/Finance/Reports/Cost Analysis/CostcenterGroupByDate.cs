using System;
using System.Drawing;
using DevExpress.DataAccess.Sql;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.XtraReports.UI;
using QD.ERP.Web.Service;
using DevExpress.XtraPrinting;
using Svg;
using System.Text;
using Microsoft.Data.SqlClient; // Make sure the namespace is correct

namespace QD.ERP.Web.Areas.Finance.Reports.Cost_Analysis
{
    public partial class CostcenterGroupByDate : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public CostcenterGroupByDate(
            string requestedBy,
            DateTime frmDate,
            DateTime toDate,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressAr,
            TenantDbContextHelper tenantDbContextHelper,string username)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            InitializeComponent();
            LoadCurrencySymbolAndImage();

            SetReportParameters(requestedBy, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr, username);
        }

        public CostcenterGroupByDate()
        {
            InitializeComponent();
            SetReportParameters(null, DateTime.MinValue, DateTime.MinValue, "", "", "", null, "", "","");
        }

        private void SetReportParameters(
            string requestedBy, DateTime frmDate, DateTime toDate,
            string tenantName, string companyName, string companyAddress,
            Image logoImage, string companyNameAr, string companyAddressAr,string username)
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

            AddOrUpdateParameter("RequestedBy", string.IsNullOrEmpty(requestedBy) ? "N/A" : requestedBy, typeof(string), !string.IsNullOrEmpty(requestedBy));
            AddOrUpdateParameter("StartDate", frmDate, typeof(DateTime), false);
            AddOrUpdateParameter("EndDate", toDate, typeof(DateTime), false);
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressAr", companyAddressAr ?? "", typeof(string));
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

            if (FindControl("xrLabelCompanyAddressAr", true) is XRLabel addressArLabel)
                addressArLabel.Text = companyAddressAr;

            if (FindControl("xrLabelRequestedBy", true) is XRLabel requestedByLabel)
            {
                requestedByLabel.Text = string.IsNullOrEmpty(requestedBy) ? "" : requestedBy;
                requestedByLabel.Visible = !string.IsNullOrEmpty(requestedBy);
            }

            ConfigureSqlDataSource(requestedBy, frmDate, toDate);
        }

        private void ConfigureSqlDataSource(string requestedBy, DateTime frmDate, DateTime toDate)
        {
            var selectQuery = new CustomSqlQuery()
            {
                Name = "qry20151CostAnalysisReport",
                Sql = @"SELECT * FROM qry20151CostAnalysisReport
                        WHERE 
                        (@RequestedBy IS NULL OR @RequestedBy = '' OR @RequestedBy = 'N/A' OR CostAllocationUnit = @RequestedBy) 
                        AND VoucherDate BETWEEN @StartDate AND @EndDate"
            };

            selectQuery.Parameters.AddRange(new[]
            {
                new QueryParameter()
                {
                    Name = "@RequestedBy",
                    Type = typeof(string),
                    ValueInfo = string.IsNullOrEmpty(requestedBy) || requestedBy == "N/A" ? "" : requestedBy
                },
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

            // Multi-tenant logic
            if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
            {
                this.sqlDataSource1.ConnectionParameters = new CustomStringConnectionParameters(tenant.ConnectionString);
            }
            else
            {
                throw new Exception("Unable to get tenant context. Please check session and cache.");
            }

            this.sqlDataSource1.Fill();
            CreateNoDataLabel();
        }

        private void CreateNoDataLabel()
        {
            XRLabel noDataLabel = new XRLabel
            {
                Text = "No records found.",
                BoundsF = new RectangleF(0, 0, PageWidth - Margins.Left - Margins.Right, 50),
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
            };
            this.Bands[BandKind.Detail].Controls.Add(noDataLabel);
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

                string[] pictureBoxNames = { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9" };

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
            string[] pictureBoxNames = { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9" };

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


    //new { Label = "xrLabel15",  Picture = "xrPictureBox6" },
    //new { Label = "xrLabel16",  Picture = "xrPictureBox5" },
    //new { Label = "xrLabel10",  Picture = "xrPictureBox9" },
    //new { Label = "xrLabel23",  Picture = "xrPictureBox7" },
    //new { Label = "xrLabel24",  Picture = "xrPictureBox8" },
    //new { Label = "xrLabel26",  Picture = "xrPictureBox4" },
    new { Label = "xrLabel00",  Picture = "xrPictureBox00" },
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
