using DevExpress.XtraPrinting;
using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace QD.ERP.Web.Areas.IMS.Reports.InventroryReports.PurchaseOrder
{
    public partial class PreviewPurchaseOrder : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
       // private bool _isApproved;
        public PreviewPurchaseOrder()
        {
            InitializeComponent();
        }

        public PreviewPurchaseOrder(
            string purchaseNo,
            string tenantName,
            string companyName,
         
            Image sealImage,
            string companyAddress,
            string companyNameAr,
            string companyAddressAr,
           // bool isApproved,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;
           // _isApproved = isApproved;

            InitializeComponent();
            SetReportParameters(purchaseNo, tenantName, companyName,  sealImage, companyAddress, companyNameAr, companyAddressAr);
            LoadReportData(purchaseNo);
        }

        private void SetReportParameters(string purchaseNo, string tenantName, string companyName,  Image sealImage,
            string companyAddress, string companyNameAr, string companyAddressAr)
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

            AddOrUpdateParameter("PurchaseNo", purchaseNo, typeof(string));
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


            if (FindControl("xrPictureBox4", true) is XRPictureBox sealPictureBox)
                sealPictureBox.Image = sealImage;
        }

        //private void LoadReportData(string purchaseOrderNo)
        //{
        //    DataTable dt = GetReportData(purchaseOrderNo);

        //    if (dt.Rows.Count == 0)
        //    {
        //        this.DataSource = null;
        //    }
        //    else
        //    {
        //        this.DataSource = dt;
        //        this.DataMember = "";
        //       // SetWatermark();

        //        decimal totalAmount = Convert.ToDecimal(dt.Compute("SUM(GrandTotal)", ""));

        //        if (FindControl("xrLabel44", true) is XRLabel labelEnglish)
        //            labelEnglish.Text = $"Amount in Words: {NumberToWordsHelper.ToEnglishWords(totalAmount)}";


        //    }
        //}
        private void LoadReportData(string purchaseOrderNo)
        {
            DataTable dt = GetReportData(purchaseOrderNo);

            if (dt.Rows.Count == 0)
            {
                this.DataSource = null;
                return;
            }

            this.DataSource = dt;
            this.DataMember = "";
            decimal totalAmount = Convert.ToDecimal(dt.Compute("SUM(GrandTotal)", ""));

            if (FindControl("xrLabel44", true) is XRLabel labelEnglish)
                labelEnglish.Text = $"Amount in Words: {NumberToWordsHelper.ToEnglishWords(totalAmount)}";

            // Get currencyId from the first row
            if (dt.Columns.Contains("currencyId") && dt.Rows[0]["currencyId"] != DBNull.Value)
            {
                int currencyId = Convert.ToInt32(dt.Rows[0]["currencyId"]);
                string svgXml = GetCurrencySvgXml(currencyId);

                if (!string.IsNullOrEmpty(svgXml))
                {
                    foreach (string pictureBoxName in new[] { "xrPictureBox5", "xrPictureBox6", "xrPictureBox7", "xrPictureBox8", "xrPictureBox9", "xrPictureBox10" , "xrPictureBox11", "xrPictureBox12"})
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
        private DataTable GetReportData(string purchaseOrderNo)
        {
            DataTable dt = new DataTable();

            try
            {
                if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    string connectionString = tenant.ConnectionString;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = "SELECT * FROM qry604_05PurchaseOrderReport WHERE PONo = @PurchaseOrderNo";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@PurchaseOrderNo", purchaseOrderNo);

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
        //private void SetWatermark()
        //{
        //    if (!_isApproved)
        //    {
        //        this.Watermark.Text = "DRAFT COPY";
        //        this.Watermark.Font = new Font("Arial", 70, FontStyle.Bold);
        //        this.Watermark.ForeColor = Color.FromArgb(80, 173, 216, 230);
        //        this.Watermark.TextDirection = DirectionMode.ForwardDiagonal;
        //        this.Watermark.ShowBehind = true;
        //        this.Watermark.ImageTiling = false;
        //        this.Watermark.ImageViewMode = ImageViewMode.Stretch;
        //    }
        //}
        private void PreviewPurchaseOrder_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // You can place custom logic before printing if needed
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
    }
}
