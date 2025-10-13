using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using QD.ERP.Web.Areas.VAT.Reports.B2B;
using QD.ERP.Web.Service;
using Svg;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace QD.ERP.Web.Areas.VAT.Reports.B2B
{
    public partial class PrintRegularTaxInvoiceWithOutDiscounts : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private bool _isApproved;  // This flag will come from the client-side

        public PrintRegularTaxInvoiceWithOutDiscounts(
            string invoiceNo,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            Image sealImage,
            string companyNameAr,
            string companyAddressAr,
            bool isApproved,  // Accepting approval status here
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _isApproved = isApproved;  // Store the approval status

            InitializeComponent();

            SetReportParameters(invoiceNo, tenantName, companyName, companyAddress, logoImage, sealImage, companyNameAr, companyAddressAr);
            LoadReportData(invoiceNo);
            LoadCurrencySymbolAndImage();
        }

        private void SetReportParameters(string invoiceNo, string tenantName, string companyName, string companyAddress, Image logoImage, Image sealImage, string companyNameAr, string companyAddressAr)
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

            AddOrUpdateParameter("InvoiceNo", invoiceNo, typeof(string));
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressAr", companyAddressAr ?? "", typeof(string));

            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = companyName;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = companyAddress;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = companyNameAr;

            if (FindControl("xrLabelCompanyAddressAr", true) is XRLabel addressArLabel)
                addressArLabel.Text = companyAddressAr;

            if (FindControl("xrPictureBox2", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;
            if (FindControl("imgCompanySeal", true) is XRPictureBox imgCompanySeal)
                imgCompanySeal.Image = sealImage;
        }

        private void LoadReportData(string invoiceNo)
        {
            DataTable dt = GetReportData(invoiceNo);  // Load data synchronously

            if (dt.Rows.Count == 0)
            {
                this.DataSource = null;

            }
            else
            {
                this.DataSource = dt;
                this.DataMember = "";
                SetWatermark();
                decimal totalAmount = Convert.ToDecimal(dt.Compute("SUM(BalanceDueAmount)", ""));


                if (FindControl("txtAmountInWords", true) is XRTableCell cellAmountInWords)
                    cellAmountInWords.Text = $"Amount in Words: {NumberToWordsHelper.ToEnglishWords(totalAmount)}";

                if (FindControl("txtAmountInWordsArabic", true) is XRTableCell cellAmountInWordsArabic)
                    cellAmountInWordsArabic.Text = $"المبلغ كتابةً: {NumberToWordsHelper.ToArabicWords(totalAmount)}";
            }
        }

        // GetReportData is implemented below:
        private DataTable GetReportData(string invoiceNo)
        {
            DataTable dt = new DataTable();

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    string connectionString = tenant.ConnectionString;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = "SELECT * FROM qry201_602VATInvoiceReport WHERE InvoiceNo = @InvoiceNo";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);

                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            conn.Open();
                            da.Fill(dt);
                        }
                    }
                }
                else
                {
                    throw new Exception("Unable to get tenant context. Please check session and cache.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching report data: {ex.Message}");
            }

            return dt;
        }


        private void SetWatermark()
        {
            if (!_isApproved)
            {
                this.Watermark.Text = "DRAFT COPY";

                // Set font
                this.Watermark.Font = new Font("Arial", 70, FontStyle.Bold);

                this.Watermark.ForeColor = Color.FromArgb(80, 173, 216, 230);

                this.Watermark.TextDirection = DirectionMode.ForwardDiagonal;

                this.Watermark.ShowBehind = true;

                this.Watermark.ImageTiling = false;
                this.Watermark.ImageViewMode = ImageViewMode.Stretch;
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

                string[] pictureBoxNames = { "xrPictureBox5", "xrPictureBox6", "xrPictureBox8", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9", "xrPictureBox10", };

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
            string[] pictureBoxNames = { "xrPictureBox8", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9", "xrPictureBox10", };

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
            var fixedPictureBoxes = new[] { "xrPictureBox7", "xrPictureBox8", "xrPictureBox9", "xrPictureBox10", "xrPictureBox11", "xrPictureBox12", "xrPictureBox13", "xrPictureBox14", "xrPictureBox15", "xrPictureBox16" };
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



        public static class NumberToWordsHelper
        {
            public static string ToEnglishWords(decimal number)
            {
                var integer = (int)number;
                var fraction = (int)((number - integer) * 100);

                string result = NumberToWords(integer);

                if (fraction > 0)
                    result += " and " + NumberToWords(fraction);

                return result + " Riyals Only";
            }

            public static string ToArabicWords(decimal number)
            {
                var integer = (int)number;
                var fraction = (int)((number - integer) * 100);

                string result = ConvertToArabic(integer);

                if (fraction > 0)
                    result += " و " + ConvertToArabic(fraction);

                return result + " فقط";
            }

            private static string ConvertToArabic(int number)
            {
                string[] ones = {
      "صفر", "واحد", "اثنان", "ثلاثة", "أربعة", "خمسة", "ستة", "سبعة", "ثمانية", "تسعة",
      "عشرة", "أحد عشر", "اثنا عشر", "ثلاثة عشر", "أربعة عشر", "خمسة عشر",
      "ستة عشر", "سبعة عشر", "ثمانية عشر", "تسعة عشر"
  };

                string[] tens = {
      "", "", "عشرون", "ثلاثون", "أربعون", "خمسون", "ستون", "سبعون", "ثمانون", "تسعون"
  };

                if (number < 20)
                    return ones[number];

                if (number < 100)
                {
                    int unit = number % 10;
                    int ten = number / 10;
                    return (unit == 0) ? tens[ten] : ones[unit] + " و " + tens[ten];
                }

                if (number < 1000)
                {
                    int hundred = number / 100;
                    int remainder = number % 100;
                    string hundredText = (hundred == 1) ? "مائة" :
                                         (hundred == 2) ? "مئتان" :
                                         ones[hundred] + " مائة";
                    return (remainder == 0) ? hundredText : hundredText + " و " + ConvertToArabic(remainder);
                }

                return number.ToString();
            }

            private static string NumberToWords(int number)
            {
                if (number == 0)
                    return "Zero";

                if (number < 0)
                    return "Minus " + NumberToWords(Math.Abs(number));

                string words = "";

                if ((number / 1000000) > 0)
                {
                    words += NumberToWords(number / 1000000) + " Million ";
                    number %= 1000000;
                }

                if ((number / 1000) > 0)
                {
                    words += NumberToWords(number / 1000) + " Thousand ";
                    number %= 1000;
                }

                if ((number / 100) > 0)
                {
                    words += NumberToWords(number / 100) + " Hundred ";
                    number %= 100;
                }

                if (number > 0)
                {
                    var unitsMap = new[] {
          "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
          "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"
      };
                    var tensMap = new[] {
          "Zero", "Ten", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"
      };

                    if (number < 20)
                        words += unitsMap[number];
                    else
                    {
                        words += tensMap[number / 10];
                        if ((number % 10) > 0)
                            words += "-" + unitsMap[number % 10];
                    }
                }

                return words.Trim();
            }
        }

        private void PrintRegularTaxInvoiceWithOutDiscounts_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
