using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using DevExpress.XtraReports.UI;
using Microsoft.Extensions.Configuration;
using QD.ERP.Web.Service;

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
            }
            else
            {
                this.DataSource = dt;
                this.DataMember = "";

                decimal totalAmount = Convert.ToDecimal(dt.Compute("SUM(DrAmount)", ""));
                int currencyId = Convert.ToInt32(dt.Rows[0]["currencyId"]);
                string currencySymbol = GetCurrencySymbol(currencyId);

                // Use currencySymbol in amount-in-words labels
                if (FindControl("xrLabel9", true) is XRLabel labelEnglish)
                {
                    // Assuming currencySymbol is already defined and available
                    labelEnglish.Text = $"Amount in Words: {NumberToWordsHelper.ToEnglishWords(totalAmount, currencySymbol)}";
                }


                if (FindControl("xrLabel10", true) is XRLabel labelArabic)
                    labelArabic.Text = $"المبلغ كتابةً: {NumberToWordsHelper.ToArabicWords(totalAmount)}";

                // Optionally: Set a label on the report to show just the currency symbol
                if (FindControl("xrLabel6", true) is XRLabel currencySymbolLabel)
                    currencySymbolLabel.Text = currencySymbol;
                if (FindControl("xrLabel17", true) is XRLabel currencySymbolLabel1)
                    currencySymbolLabel1.Text = currencySymbol;
                if (FindControl("xrLabel18", true) is XRLabel currencySymbolLabel2)
                    currencySymbolLabel2.Text = currencySymbol;
                if (FindControl("xrLabel19", true) is XRLabel currencySymbolLabel3)
                    currencySymbolLabel3.Text = currencySymbol;
            }
        }
        private string GetCurrencySymbol(int currencyId)
        {
            string symbol = "";

            try
            {
                if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                    throw new Exception("Unable to retrieve tenant context.");

                string connectionString = tenant.ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT CurrencyCode FROM Tbl20169CurrencyExchange WHERE CurrencyExchangeId = @currencyId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@currencyId", currencyId);
                        conn.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null)
                            symbol = result.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving currency symbol: {ex.Message}");
            }

            return symbol;
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

    }
}
