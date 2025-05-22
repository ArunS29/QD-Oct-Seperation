using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace QD.ERP.Web.Areas.VAT.Reports.ProformaInvoices
{
    public partial class ProformaInvoiceForeignEnglish : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private bool _isApproved;

        public ProformaInvoiceForeignEnglish(
            string invoiceNo,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressAr,
          string companyPhone,
            string companyEmail,
            string companyWebsite,
            bool isApproved,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            _isApproved = isApproved;

            InitializeComponent();
            SetReportParameters(invoiceNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr,companyPhone, companyEmail,companyWebsite);
            LoadReportData(invoiceNo);
        }

        private void SetReportParameters(string invoiceNo, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressAr,
    string companyPhone , string companyEmail, string companyWebsite)
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
            AddOrUpdateParameter("CompanyPhone", companyPhone ?? "", typeof(string));
            AddOrUpdateParameter("CompanyEmailAddress", companyEmail?? "", typeof(string));
           
            AddOrUpdateParameter("CompanyWebsite", companyWebsite ?? "", typeof(string));

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
            if (FindControl("xrLabelCompanyPhone", true) is XRLabel companyphoneLabel)
                companyphoneLabel.Text = companyPhone;

            if (FindControl("xrLabelCompanyEmailAddress", true) is XRLabel emailLabel)
                emailLabel.Text = companyEmail;
        
            if (FindControl("xrLabelCompanyWebsite", true) is XRLabel websiteLabel)
                websiteLabel.Text = companyWebsite;


            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;
        }

        private void LoadReportData(string invoiceNo)
        {
            DataTable dt = GetReportData(invoiceNo);

            if (dt.Rows.Count == 0)
            {
                this.DataSource = null;
                CreateNoDataLabel();
            }
            else
            {
                this.DataSource = dt;
                this.DataMember = "";
                SetWatermark();

                decimal totalAmount = Convert.ToDecimal(dt.Compute("SUM(DrAmount)", ""));

                if (FindControl("xrLabel35", true) is XRLabel labelEnglish)
                    labelEnglish.Text = "Amount in Words: " + NumberToWordsHelper.ToEnglishWords(totalAmount);

                if (FindControl("xrLabel10", true) is XRLabel labelArabic)
                   labelArabic.Text = "المبلغ كتابةً: " + NumberToWordsHelper.ToArabicWords(totalAmount);
            }
        }

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
                        string query = "SELECT * FROM qry201_652ProformaInvoiceReport WHERE InvoiceNo = @ProformaInvoiceNo";

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
                this.Watermark.Font = new Font("Arial", 70, FontStyle.Bold);
                this.Watermark.ForeColor = Color.FromArgb(80, 173, 216, 230);
                this.Watermark.TextDirection = DirectionMode.ForwardDiagonal;
                this.Watermark.ShowBehind = true;
                this.Watermark.ImageTiling = false;
                this.Watermark.ImageViewMode = ImageViewMode.Stretch;
            }
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

        public static class NumberToWordsHelper
        {
            public static string ToEnglishWords(decimal number)
            {
                var integer = (int)number;
                var fraction = (int)((number - integer) * 100);

                string result = NumberToWords(integer) + " Riyals";

                if (fraction > 0)
                    result += " and " + NumberToWords(fraction) + " Halalas";

                return result + " Only";
            }

            public static string ToArabicWords(decimal number)
            {
                var integer = (int)number;
                var fraction = (int)((number - integer) * 100);

                string result = ConvertToArabic(integer) + " ريال";

                if (fraction > 0)
                    result += " و " + ConvertToArabic(fraction) + " هللة";

                return result + " فقط";
            }

            private static string ConvertToArabic(int number)
            {
                // Simplified mapping — You may replace this with full Arabic logic or library
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

                // For simplicity, add more conditions if needed for thousands/millions
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
    }
}
