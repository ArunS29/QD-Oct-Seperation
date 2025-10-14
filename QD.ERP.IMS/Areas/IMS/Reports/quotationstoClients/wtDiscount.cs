using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Microsoft.Extensions.Configuration;

using QD.ERP.IMS.Areas.IMS.Reports.quotationstoClients;
using QD.ERP.Shared.Service;
using Svg;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;

namespace QD.ERP.IMS.Areas.IMS.Reports.quotationstoClients
{
    public partial class wtDiscount : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly Tenant _resolvedTenant;
        private HashSet<string> _quoteNosWithTerms = new HashSet<string>();
        public wtDiscount()
        {
            InitializeComponent();
        }

        public wtDiscount(
               bool showSeal,
           bool showSignature,
           bool printLetterhead,
            bool pageBreakBefore,
             bool pageBreakAfter,
            bool clientAcknowledgement,
            bool printItemCodeDesc,
            bool printItemPartNoDesc,
            bool printItemPartArabicDesc,
            string quotationNo,
            string tenantName,
            string companyName,
            Image logoImage,
            Image sealImage,
            string companyAddress,
          
            string companyNameAr,
            string companyAddressAr,
            string username,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out _resolvedTenant, out _))
                throw new Exception("Unable to resolve tenant context during report creation.");


            InitializeComponent();
            SetReportParameters(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, printItemCodeDesc, printItemPartNoDesc, printItemPartArabicDesc, quotationNo, tenantName, companyName, logoImage, sealImage,
                companyAddress,
                companyNameAr, companyAddressAr,username);

            LoadReportData(quotationNo);
            LoadSubreport(quotationNo);

            xrSubreport1.BeforePrint += xrSubreport1_BeforePrint;



            if (pageBreakBefore || pageBreakAfter)
                this.ReportFooter.PageBreak = DevExpress.XtraReports.UI.PageBreak.BeforeBand;
            else
                this.ReportFooter.PageBreak = DevExpress.XtraReports.UI.PageBreak.None;

            ApplyConditionalVisibility(showSeal, showSignature, printLetterhead);


            // Hide labels if clientAcknowledgement is true
            int[] labelNumbers = { 38, 39, 61, 62, 63, 64, 65, 66, 67, 68, 69, 71, 72, 73, 70 };
            foreach (int num in labelNumbers)
            {
                var label = FindControl($"xrLabel{num}", true) as XRLabel;
                if (label != null)
                    label.Visible = clientAcknowledgement;
            }
        }

        private void xrSubreport1_BeforePrint(object sender, EventArgs e)
        {
            string quoteNo = GetCurrentColumnValue("QuoteNo")?.ToString()
                     ?? Parameters["QuotationNo"].Value?.ToString();

            var tenant = _resolvedTenant;
            var subReport = new IMSContext();
            if (!string.IsNullOrWhiteSpace(quoteNo) && tenant != null)
            {
                subReport.LoadTerms(quoteNo, tenant.ConnectionString);
                xrSubreport1.ReportSource = subReport;

                // Check if subreport has data
                var dt = subReport.DataSource as DataTable;
                xrSubreport1.Visible = dt != null && dt.Rows.Count > 0;
            }
            else
            {
                xrSubreport1.Visible = false;
            }
        }
        private void LoadSubreport(string quotationNo)
        {
            if (string.IsNullOrWhiteSpace(quotationNo)) return;

            var tenant = _resolvedTenant ?? throw new Exception("Tenant not resolved.");
            DataTable dt = new DataTable();

            using (var conn = new SqlConnection(tenant.ConnectionString))
            {
                // Adjust the query to match your subreport's data needs
                string query = "SELECT DISTINCT QuoteNo FROM qry601_05QuotationReport WHERE QuoteNo = @QuotationNo";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@QuotationNo", quotationNo);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            foreach (DataRow row in dt.Rows)
            {
                string quoteNo = row["QuoteNo"]?.ToString();
                if (!string.IsNullOrWhiteSpace(quoteNo))
                {
                    _quoteNosWithTerms.Add(quoteNo);
                }
            }
        }


        private void SetReportParameters(bool showSeal, bool showSignature, bool printLetterhead, bool pageBreakAfter, bool pageBreakBefore, bool printItemCodeDesc, bool printItemPartNoDesc, bool printItemPartArabicDesc, string quotationNo, string tenantName, string companyName,
            Image logoImage, Image sealImage, string companyAddress, string companyNameAr, string companyAddressAr,string username)
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

            AddOrUpdateParameter("QuotationNo", quotationNo, typeof(string));
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressAr", companyAddressAr ?? "", typeof(string));
            AddOrUpdateParameter("UserName", username ?? "", typeof(string));
            AddOrUpdateParameter("printItemCodeDesc", printItemCodeDesc, typeof(bool), false);
            AddOrUpdateParameter("printItemPartNoDesc", printItemPartNoDesc, typeof(bool), false);
            AddOrUpdateParameter("printItemPartArabicDesc", printItemPartArabicDesc, typeof(bool), false);

            if (FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = companyName;

            if (FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = companyAddress;
            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel arCompanyNameLabel)
                arCompanyNameLabel.Text = companyNameAr;
            if (FindControl("UserName", true) is XRLabel usernameLabel)
                usernameLabel.Text = username;

            if (FindControl("xrLabelCompanyAddressAr", true) is XRLabel arAddressLabel)
                arAddressLabel.Text = companyAddressAr;

            if (FindControl("xrPictureBox15", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;

            if (FindControl("xrPictureBox1", true) is XRPictureBox sealPictureBox)
                sealPictureBox.Image = sealImage;
        }

        //private void LoadReportData(string quotationNo)
        //{
        //    DataTable dt = GetReportData(quotationNo);

        //    if (dt.Rows.Count == 0)
        //    {
        //        this.DataSource = null;
        //    }
        //    else
        //    {
        //        this.DataSource = dt;
        //        this.DataMember = ""; // Set to table name if needed

        //        //2 FinalTotal
        //        decimal totalAmount = Convert.ToDecimal(dt.Compute("SUM(FinalTotal)", ""));

        //        if (FindControl("xrLabel2", true) is XRLabel labelEnglish)
        //            labelEnglish.Text = $"Amount in Words: {NumberToWordsHelper.ToEnglishWords(totalAmount)}";


        //    }
        //}
        private void LoadReportData(string quotationNo)
        {
            DataTable dt = GetReportData(quotationNo);

            if (dt.Rows.Count == 0)
            {
                this.DataSource = null;
                return;
            }

            this.DataSource = dt;
            this.DataMember = "";
            decimal totalAmount = Convert.ToDecimal(dt.Compute("SUM(FinalTotal)", ""));

            if (FindControl("xrLabel2", true) is XRLabel labelEnglish)
                labelEnglish.Text = $"Amount in Words: {NumberToWordsHelper.ToEnglishWords(totalAmount)}";

            // Get currencyId from the first row
            if (dt.Columns.Contains("currencyId") && dt.Rows[0]["currencyId"] != DBNull.Value)
            {
                int currencyId = Convert.ToInt32(dt.Rows[0]["currencyId"]);
                string svgXml = GetCurrencySvgXml(currencyId);

                if (!string.IsNullOrEmpty(svgXml))
                {
                    foreach (string pictureBoxName in new[] { "xrPictureBox2", "xrPictureBox12", "xrPictureBox13", "xrPictureBox14", "xrPictureBox11", "xrPictureBox8", "xrPictureBox9", "xrPictureBox10","xrPictureBox3", "xrPictureBox4" })
                    {
                        if (FindControl(pictureBoxName, true) is XRPictureBox pictureBox)
                        {
                            try
                            {
                                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(svgXml)))
                                {
                                    var svgDoc = Svg.SvgDocument.Open<Svg.SvgDocument>(stream);
                                    Bitmap bitmap = svgDoc.Draw();
                                    pictureBox.Image = bitmap;
                                    pictureBox.Sizing = ImageSizeMode.ZoomImage;
                                    pictureBox.SizeF = new SizeF(15f, 15f); // adjust as needed
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Failed to render SVG for {pictureBoxName}: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }

        private void ApplyConditionalVisibility(bool showSeal, bool showSignature, bool printLetterhead)
        {
            // 🔹 Seal logic (xrPictureBox1)
            if (FindControl("xrPictureBox1", true) is XRPictureBox sealPicture)
                sealPicture.Visible = showSeal;

            // 🔹 Signature logic (xrPictureBox5, xrPictureBox6, xrPictureBox7)
            foreach (string signatureBox in new[] { "xrPictureBox5", "xrPictureBox6", "xrPictureBox7" })
            {
                if (FindControl(signatureBox, true) is XRPictureBox sigBox)
                    sigBox.Visible = showSignature;
            }

            // 🔹 Letterhead logic (xrLabel75, xrLabel76, xrPictureBox11, xrLine3)
            if (FindControl("xrLabel80", true) is XRLabel lbl75)
                lbl75.Visible = printLetterhead;

            if (FindControl("xrLabel81", true) is XRLabel lbl76)
                lbl76.Visible = printLetterhead;

            if (FindControl("xrLabel83", true) is XRLabel lbl83)
                lbl83.Visible = printLetterhead;
            if (FindControl("xrLabel85", true) is XRLabel lbl85)
                lbl85.Visible = printLetterhead;
            if (FindControl("xrLabel86", true) is XRLabel lbl86)
                lbl86.Visible = printLetterhead;
            if (FindControl("xrLabel87", true) is XRLabel lbl87)
                lbl87.Visible = printLetterhead;
            if (FindControl("xrLabel88", true) is XRLabel lbl88)
                lbl88.Visible = printLetterhead;

            if (FindControl("xrPictureBox15", true) is XRPictureBox logoBox)
                logoBox.Visible = printLetterhead;

            if (FindControl("xrLine5", true) is XRLine line3)
                line3.Visible = printLetterhead;
        }
        private DataTable GetReportData(string quotationNo)
        {
            DataTable dt = new DataTable();

            try
            {
                if (_tenantDbContextHelper != null &&
                    _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    string connectionString = tenant.ConnectionString;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = "SELECT * FROM qry601_05QuotationReport WHERE QuoteNo = @QuotationNo";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@QuotationNo", quotationNo);

                            SqlDataAdapter da = new SqlDataAdapter(cmd);
                            conn.Open();
                            da.Fill(dt);
                        }
                    }
                }
                else
                {
                    throw new Exception("Tenant context not found. Please check the session/cache.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading wtDiscount data: " + ex.Message);
            }

            return dt;
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

                return result + " Only";
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
    }
}

