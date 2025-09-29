using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.DataFederation;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraReports.UI;
using QD.ERP.Web.Service;
using System;
using System.Drawing;
using System.Data;
using DevExpress.XtraPrinting;
using Svg;
using System.Text;
using Microsoft.Data.SqlClient;

namespace QD.ERP.Web.Areas.Finance.Reports.Payable_Statements
{
    public partial class Balance : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public Balance(
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
            TenantDbContextHelper tenantDbContextHelper
        )
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            InitializeComponent();
            SetReportParameters(accountId, frmDate, toDate, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb,username);
            ConfigureDataSource(accountId);
            LoadCurrencySymbolAndImage();

        }

        public Balance()
        {
            InitializeComponent();
        }

        private void ConfigureDataSource(string accountId)
        {
            sqlDataSource1.Queries.Clear();
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                throw new Exception("Unable to get tenant context. Please check session and cache.");

            var connectionParams = new CustomStringConnectionParameters(tenant.ConnectionString);
            sqlDataSource1 = new SqlDataSource(connectionParams);

            var query = new CustomSqlQuery
            {
                Name = "qry205_017AgeingBillsPayableWtColumns", // Change to match your report
                Sql = "SELECT * FROM qry205_017AgeingBillsPayableWtColumns WHERE AccountHeadNo = @AccountID"
            };

            query.Parameters.Add(new QueryParameter
            {
                Name = "AccountID",
                Type = typeof(string),
                Value = accountId ?? ""
            });

            sqlDataSource1.Queries.Clear();
            sqlDataSource1.Queries.Add(query);

            sqlDataSource1.RebuildResultSchema(); // Optional
            sqlDataSource1.Fill(); // Important

            this.DataSource = sqlDataSource1;
            this.DataMember = "qry205_017AgeingBillsPayableWtColumns"; // Match query name
                                                                       // Fill data and handle no-data case
            try
            {
                sqlDataSource1.Fill();

                // Check if data exists
                var data = sqlDataSource1.Result["qry205_017AgeingBillsPayableWtColumns"];
                if (data == null || !data.Cast<object>().Any())
                {
                    this.DataSource = null;
                    CreateNoDataLabel();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading data: " + ex.Message, ex);
            }
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
            string companyAddressArb,string username)
        {
            void AddOrUpdateParameter(string paramName, object paramValue, Type paramType, bool visible)
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

            AddOrUpdateParameter("AccountID", accountId, typeof(string), false);
            AddOrUpdateParameter("StartDate", frmDate, typeof(DateTime), false);
            AddOrUpdateParameter("EndDate", toDate, typeof(DateTime), false);
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string), false);
            AddOrUpdateParameter("CompanyAddressArb", companyAddressArb ?? "", typeof(string), false);
            AddOrUpdateParameter("xrLabelTenantName", username ?? "", typeof(string), false);
         
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
            if (this.FindControl("xrLabelUserName", true) is XRLabel userNameLabel)
                userNameLabel.Text = username;

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox && logoImage != null)
                logoPictureBox.Image = logoImage;
        }
        private void CheckForEmptyData()
        {
            if (sqlDataSource1?.Result != null && !string.IsNullOrEmpty(DataMember))
            {
                var resultTable = sqlDataSource1.Result[DataMember] as System.Data.DataTable;
                if (resultTable == null || resultTable.Rows.Count == 0)
                {
                    XRLabel noDataLabel = new XRLabel()
                    {
                        Text = "No records found to display.",
                        BoundsF = new RectangleF(0, 0, 850, 50),
                        TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter,
                        Font = new Font("Arial", 14, FontStyle.Bold),
                        ForeColor = Color.Red
                    };

                    this.Bands[BandKind.Detail].Controls.Clear();
                    this.Bands[BandKind.Detail].Controls.Add(noDataLabel);
                }
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

                string[] pictureBoxNames = { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9",
                    "xrPictureBox10", "xrPictureBox11", "xrPictureBox12", "xrPictureBox13", "xrPictureBox14", "xrPictureBox15", "xrPictureBox16", "xrPictureBox17" ,
                    "xrPictureBox18", "xrPictureBox19", "xrPictureBox20", "xrPictureBox21" };

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
            string[] pictureBoxNames ={ "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9",
                    "xrPictureBox10", "xrPictureBox11", "xrPictureBox12", "xrPictureBox13", "xrPictureBox14", "xrPictureBox15", "xrPictureBox16", "xrPictureBox17" ,
                    "xrPictureBox18", "xrPictureBox19", "xrPictureBox20", "xrPictureBox21" };

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
            var fixedPictureBoxes = new[] { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4","xrPictureBox5", "xrPictureBox6", "xrPictureBox7",
                "xrPictureBox18", "xrPictureBox19", "xrPictureBox20","xrPictureBox21" };
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


    new { Label = "xrLabel6",  Picture = "xrPictureBox8" },
    new { Label = "xrLabel12",  Picture = "xrPictureBox9" },
    new { Label = "xrLabel13",  Picture = "xrPictureBox10" },
    new { Label = "xrLabel14",  Picture = "xrPictureBox11" },
    new { Label = "xrLabel15",  Picture = "xrPictureBox12" },
    new { Label = "xrLabel16",  Picture = "xrPictureBox13" },
    new { Label = "xrLabel17",  Picture = "xrPictureBox14" },
    new { Label = "xrLabel18",  Picture = "xrPictureBox15" },
    new { Label = "xrLabel19",  Picture = "xrPictureBox16" },
  
    new { Label = "xrLabel8",  Picture = "xrPictureBox17" },
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
        private void PageFooter_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {


        }
           private void CreateNoDataLabel()
        {
            // Clear all existing controls from Detail band first
            this.Bands[BandKind.Detail].Controls.Clear();
            
            // Create a prominent no-data label that will be visible
            XRLabel noDataLabel = new XRLabel
            {
                Text = "No records found.",
                BoundsF = new RectangleF(100, 30, PageWidth - 200, 80),
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter,
                Font = new Font("Arial", 18, FontStyle.Bold),
                ForeColor = Color.Red,
                BackColor = Color.LightYellow,
                Borders = DevExpress.XtraPrinting.BorderSide.All,
                BorderColor = Color.Red,
                BorderWidth = 2,
                Padding = new DevExpress.XtraPrinting.PaddingInfo(15, 15, 15, 15, 100f)
            };
            
            // Set higher Z-order to bring to front
            noDataLabel.BringToFront();
            
            // Add to Detail band
            this.Bands[BandKind.Detail].Controls.Add(noDataLabel);
            
            // Hide headers and footers that might interfere
           
            
            // Make Detail band taller and ensure it's visible
            this.Bands[BandKind.Detail].HeightF = 140f;
            this.Bands[BandKind.Detail].Visible = true;
}
    }
}
