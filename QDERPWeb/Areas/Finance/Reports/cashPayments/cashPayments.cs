using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Text;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Microsoft.Extensions.Configuration;
using QD.ERP.Web.Areas.Finance.Reports.cashPayments;
using QD.ERP.Web.Service;
using Svg;
using System.Drawing.Printing;



namespace QD.ERP.Web.Areas.Finance.Reports.test
{
    public partial class cashPayments : XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public cashPayments(
            string voucherNo,
            string tenantName,
            string company_Name,
            string company_address,
            Image logoImage,
            string Company_Name_Ar,
            string company_address_arb,
            string username,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;

            InitializeComponent();
            SetReportParameters(voucherNo, tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb,username);
            LoadReportData(voucherNo);
            LoadSubreport(voucherNo);


        }
        private void LoadSubreport(string voucherNo)
        {
            if (string.IsNullOrWhiteSpace(voucherNo)) return;

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out _))
                throw new Exception("Unable to retrieve tenant context.");

            var subReport = new subCostReport();
            subReport.LoadData(voucherNo, tenant.ConnectionString);

            // ✅ Set the subreport directly
            xrSubreport1.ReportSource = subReport;
        }


        private void SetReportParameters(string voucherNo, string tenantName, string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb, string username)
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

            AddOrUpdateParameter("VoucherNo", voucherNo, typeof(string));
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("UserName", username ?? "", typeof(string), false);

            AddOrUpdateParameter("CompanyName", company_Name ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", company_address ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", Company_Name_Ar ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressArb", company_address_arb ?? "", typeof(string));

            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;
            if (FindControl("xrLabelUserName", true) is XRLabel userLabel)
                userLabel.Text = username;
            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = company_Name;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = company_address;

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = Company_Name_Ar;

            if (FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
                addressArbLabel.Text = company_address_arb;

            if (FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;
        }

        private void LoadReportData(string voucherNo)
        {
            DataTable dt = GetReportData(voucherNo);

            if (dt.Rows.Count == 0)
            {
                this.DataSource = null;
                CreateNoDataLabel();
                return;
            }

            this.DataSource = dt;
            this.DataMember = "";

            decimal totalAmount = Convert.ToDecimal(dt.Compute("SUM(DrAmount)", ""));
            int currencyId = Convert.ToInt32(dt.Rows[0]["currencyId"]);

            var currencyInfo = GetCurrencySymbolOrImageStatus(currencyId);

            // Amount in words
            if (FindControl("xrLabel9", true) is XRLabel labelEnglish)
                labelEnglish.Text = $"Amount in Words: {NumberToWordsHelper.ToEnglishWords(totalAmount, currencyInfo.Symbol)}";

            if (FindControl("xrLabel10", true) is XRLabel labelArabic)
                labelArabic.Text = $"المبلغ كتابةً: {NumberToWordsHelper.ToArabicWords(totalAmount)}";

            if (!string.IsNullOrEmpty(currencyInfo.Symbol))
            {
                foreach (string labelName in new[] { "xrLabel6", "xrLabel17", "xrLabel18", "xrLabel19" })
                {
                    if (FindControl(labelName, true) is XRLabel label)
                        label.Text = currencyInfo.Symbol;
                }
            }
            else if (currencyInfo.HasImage)
            {
                string svgXml = GetCurrencySvgXml(currencyId);
                if (!string.IsNullOrEmpty(svgXml))
                {
                    foreach (string pictureBoxName in new[] { "xrPictureBox2", "xrPictureBox3", "xrPictureBox4", "xrPictureBox5" })
                    {
                        if (FindControl(pictureBoxName, true) is XRPictureBox pictureBox)
                        {
                            try
                            {
                                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgXml)))
                                {
                                    SvgDocument svgDoc = SvgDocument.Open<SvgDocument>(stream);
                                    Bitmap bitmap = svgDoc.Draw(); // original quality

                                    pictureBox.Image = bitmap;
                                    pictureBox.Sizing = ImageSizeMode.Normal; // Best for scaling inside the box
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to render SVG: {ex.Message}");
                            }
                        }
                    }

                }
            }
            
        }


        private (string Symbol, bool HasImage) GetCurrencySymbolOrImageStatus(int currencyId)
        {
            string symbol = "";
            bool hasImage = false;

            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                    throw new Exception("Unable to retrieve tenant context.");

                string connectionString = tenant.ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                SELECT CurrencySymbole, CurrencyImage 
                FROM Tbl20169CurrencyExchange 
                WHERE CurrencyExchangeId = @currencyId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@currencyId", currencyId);
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                symbol = reader["CurrencySymbole"]?.ToString();
                                hasImage = reader["CurrencyImage"] != DBNull.Value && !string.IsNullOrWhiteSpace(reader["CurrencyImage"]?.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving currency info: {ex.Message}");
            }

            return (symbol?.Trim() ?? "", hasImage);
        }


        private string GetCurrencySvgXml(int currencyId)
        {
            string svgXml = null;

            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                    throw new Exception("Unable to retrieve tenant context.");

                string connectionString = tenant.ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT CurrencyImage FROM Tbl20169CurrencyExchange WHERE CurrencyExchangeId = @currencyId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@currencyId", currencyId);
                        conn.Open();

                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                            svgXml = result.ToString(); // Raw SVG XML as string
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving currency SVG: {ex.Message}");
            }

            return svgXml;
        }



        private DataTable GetReportData(string voucherNo)
        {
            DataTable dt = new DataTable();

            try
            {
                // Get multi-tenant connection string
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                    throw new Exception("Unable to retrieve tenant context. Please check session or cache.");

                string connectionString = tenant.ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM [qry201MainVoucherEntriesWithMaster] WHERE voucherno = @VoucherNo";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@VoucherNo", voucherNo);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        conn.Open();
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching report data: {ex.Message}");
            }

            return dt;
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
            public static string ToEnglishWords(decimal number, string currencySymbol)
            {
                var integer = (int)number;
                var fraction = (int)((number - integer) * 100);

                string result = NumberToWords(integer) + " " + currencySymbol;

                if (fraction > 0)
                    result += " and " + NumberToWords(fraction) + " " + currencySymbol;

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
        // Change the event handler signature to match DevExpress's BeforePrint event
        

    }
}
