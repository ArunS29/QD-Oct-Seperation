using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.Parameters;
using DevExpress.XtraReports.UI;
using Svg;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;


namespace QD.ERP.Web.Areas.Finance.Reports.ImportReports.PayableandReceivable
{
    public partial class XtraRecivableReport2Import : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public XtraRecivableReport2Import(
             string username,
            object[] selectedValues,
            string selectionType, // <-- Add this
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressArb,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            InitializeComponent();
            SetReportParameters(username, selectedValues, selectionType, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressArb);
            LoadCurrencySymbolAndImage();
        }

        public XtraRecivableReport2Import()
        {
            InitializeComponent();
            SetReportParameters("", null, "", "", "", "", null, "", "");
        }

        private void SetReportParameters(string username, object[] selectedValues, string selectionType, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressArb)
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
            AddOrUpdateParameter("SelectedValues", selectedValues ?? new object[0], typeof(object[]));
            AddOrUpdateParameter("SelectionType", selectionType ?? "", typeof(string));

            SetLabelText("xrLabelTenantName", tenantName);
            SetLabelText("xrLabelCompanyName", companyName);
            SetLabelText("xrLabelCompanyAddress", companyAddress);
            SetLabelText("xrLabelCompanyNameAr", companyNameAr);
            SetLabelText("xrLabelCompanyAddressArb", companyAddressArb);
            AddOrUpdateParameter("UserName", username ?? "", typeof(string), false);
            if (FindControl("xrLabelUserName", true) is XRLabel userNameLabel)
                userNameLabel.Text = username;

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
            {
                logoPictureBox.Image = logoImage;
                logoPictureBox.Visible = logoImage != null;
            }

            // Multitenant-aware data loading
            LoadReportData(selectedValues, selectionType); // Pass selectionType
        }

        private void SetLabelText(string controlName, string text)
        {
            if (FindControl(controlName, true) is XRLabel label)
                label.Text = text ?? "";
        }

        private void LoadReportData(object[] selectedValues, string selectionType)
        {
            if (selectedValues == null || selectedValues.Length == 0)
            {
                Console.WriteLine("No selection provided. Loading full report.");
                return;
            }

            string firstValue = selectedValues.First()?.ToString();
            if (string.IsNullOrWhiteSpace(firstValue))
            {
                Console.WriteLine("Invalid first value. Report data not loaded.");
                return;
            }

            string selectedFilter = string.Join(",", selectedValues.Select(val => $"'{val.ToString().Replace("'", "''")}'"));

            CustomSqlQuery selectQuery = new CustomSqlQuery();

            if (selectionType == "AccountHead")
            {
                // Only for AccountHead, check for 'L'
                if (!firstValue.StartsWith("L"))
                {
                    Console.WriteLine("Invalid AccountHeadNo format.");
                    return;
                }
                selectQuery.Name = "tbl201SubLedgerPayablesMaster";
                selectQuery.Sql = $"SELECT * FROM tbl201SubLedgerPayablesMaster WHERE AccountHeadNo IN ({selectedFilter})";
            }
            else if (selectionType == "SalesPerson")
            {
                selectQuery.Name = "tbl201SubLedgerPayablesMaster";
                selectQuery.Sql = $"SELECT * FROM tbl201SubLedgerPayablesMaster WHERE SalesPersonName IN ({selectedFilter})";
            }
            else if (selectionType == "Branch")
            {
                selectQuery.Name = "tbl201SubLedgerPayablesMaster";
                selectQuery.Sql = $"SELECT * FROM tbl201SubLedgerPayablesMaster WHERE DivisionName IN ({selectedFilter})";
            }
            else
            {
                Console.WriteLine("Invalid selection type. No report generated.");
                return;
            }

            try
            {
                if (this.sqlDataSource1 == null)
                    this.sqlDataSource1 = new SqlDataSource();

                this.sqlDataSource1.Queries.Clear();
                this.sqlDataSource1.Queries.Add(selectQuery);

                // Multitenant database connection setup
                if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    var connectionParams = new CustomStringConnectionParameters(tenant.ConnectionString);
                    this.sqlDataSource1.ConnectionParameters = connectionParams;
                }
                else
                {
                    throw new Exception("Unable to get tenant context. Please check session and cache.");
                }

                this.sqlDataSource1.RebuildResultSchema();
                this.sqlDataSource1.Fill();

                this.DataSource = sqlDataSource1;
                this.DataMember = selectQuery.Name;

                Console.WriteLine("Report Data Loaded Successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Loading Report Data: {ex.Message}");
            }
        }

        private string FormatSelectedFilter(object[] filters)
        {
            return string.Join(",", filters.Select(x => $"'{x.ToString().Trim().Replace("'", "''")}'"));
        }

        private bool IsSalesPersonCode(string value)
        {
            return int.TryParse(value, out int num) && (num >= 100 && num <= 999);
        }

        private bool IsBranchCode(string value)
        {
            return int.TryParse(value, out int num) && (num >= 1 && num <= 99);
        }
        private bool IsSalesPersonName(List<string> values)
        {

            return values.All(v => !string.IsNullOrWhiteSpace(v) && !v.StartsWith("L") && !v.All(char.IsDigit));
        }

        private bool IsBranchName(List<string> values)
        {
            return values.All(v => !string.IsNullOrWhiteSpace(v) && !v.StartsWith("L") && !v.All(char.IsDigit));
        }

        protected override void OnDataSourceDemanded(EventArgs e)
        {
            base.OnDataSourceDemanded(e);
            Console.WriteLine("Report DataSource Demanded.");
        }

        private void xrTableCell4_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {

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
        private void RemoveGroupHeaders(bool showByBranch, bool showBySalesPerson, bool showByCompany)
        {
            try
            {
                // Example: assumes you have these group headers defined in the report designer
                var groupHeaderBranch = this.Bands["GroupHeaderBranch"];
                var groupHeaderSalesPerson = this.Bands["GroupHeaderSalesPerson"];
                var groupHeaderCompany = this.Bands["GroupHeaderCompany"];

                if (showByCompany)
                {
                    if (groupHeaderBranch != null) this.Bands.Remove(groupHeaderBranch);
                    if (groupHeaderSalesPerson != null) this.Bands.Remove(groupHeaderSalesPerson);
                }
                else
                {
                    if (showByBranch && groupHeaderBranch != null)
                        this.Bands.Remove(groupHeaderBranch);

                    if (showBySalesPerson && groupHeaderSalesPerson != null)
                        this.Bands.Remove(groupHeaderSalesPerson);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing group headers: {ex.Message}");
            }
        }
        private void XtraRecivableReport_BeforePrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            try
            {
                // Example flags – you can bind these from your report parameters or elsewhere
                bool showByBranch = Convert.ToBoolean(Parameters["ShowReportByBranch"]?.Value ?? false);
                bool showBySalesPerson = Convert.ToBoolean(Parameters["ShowReportBySalesPerson"]?.Value ?? false);
                bool showByCompany = Convert.ToBoolean(Parameters["ShowReportByCompany"]?.Value ?? false);

                RemoveGroupHeaders(showByBranch, showBySalesPerson, showByCompany);

                if (FindControl("xrLabelUserPrinting", true) is XRLabel lblUser)
                    lblUser.Text = $"Reported By: {Parameters["UserName"]?.Value} on {DateTime.Now:dd-MMM-yyyy hh:mm tt}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BeforePrint: {ex.Message}");
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
            var fixedPictureBoxes = new[] { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5", };
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
