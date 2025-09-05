using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace QD.ERP.Web.Areas.IMS.Reports.InventroryReports.Delivery_Note
{
    public partial class PreviewDeliveryNotewithPrice : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public PreviewDeliveryNotewithPrice()
        {
            InitializeComponent();
        }

        public PreviewDeliveryNotewithPrice(
             bool PrintFooterAtBottom,
 bool ShowItemLineNo,
 bool printItemPartArabicDesc,
 bool showSeal,
 bool showSignature,
 bool printLetterhead,
 bool printItemCodeDesc,
 bool printItemPartNoDesc,
            string deliveryNoteNo,
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

            InitializeComponent();
            SetReportParameters(PrintFooterAtBottom, ShowItemLineNo, printItemPartArabicDesc, showSeal, showSignature, printLetterhead, printItemCodeDesc, printItemPartNoDesc, deliveryNoteNo, tenantName, companyName, logoImage, sealImage, companyAddress, companyNameAr, companyAddressAr, username);
            LoadReportData(deliveryNoteNo);

            ApplyConditionalVisibility(showSeal, showSignature, printLetterhead, PrintFooterAtBottom);
        }

        private void SetReportParameters(bool PrintFooterAtBottom, bool ShowItemLineNo, bool printItemPartArabicDesc, bool showSeal, bool showSignature, bool printLetterhead, bool printItemCodeDesc, bool printItemPartNoDesc,
            string deliveryNoteNo, string tenantName, string companyName, Image logoImage, Image sealImage,
            string companyAddress, string companyNameAr, string companyAddressAr, string username)
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

            AddOrUpdateParameter("DeliveryNoteNo", deliveryNoteNo, typeof(string));
            AddOrUpdateParameter("TenantName", tenantName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressAr", companyAddressAr ?? "", typeof(string));
            AddOrUpdateParameter("UserName", username ?? "", typeof(string));

            AddOrUpdateParameter("printItemCodeDesc", printItemCodeDesc, typeof(bool), false);
            AddOrUpdateParameter("printItemPartNoDesc", printItemPartNoDesc, typeof(bool), false);
            AddOrUpdateParameter("printItemPartArabicDesc", printItemPartArabicDesc, typeof(bool), false);
            AddOrUpdateParameter("ShowItemLineNo", ShowItemLineNo, typeof(bool), false);


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

            if (FindControl("UserName", true) is XRLabel usernameLabel)
                usernameLabel.Text = username;

            if (FindControl("xrPictureBox11", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;

            if (FindControl("xrPictureBox1", true) is XRPictureBox sealPictureBox)
                sealPictureBox.Image = sealImage;
        }

        //private void LoadReportData(string deliveryNoteNo)
        //{
        //    DataTable dt = GetReportData(deliveryNoteNo);

        //    if (dt.Rows.Count == 0)
        //    {
        //        this.DataSource = null;
        //    }
        //    else
        //    {
        //        this.DataSource = dt;
        //        this.DataMember = ""; // or set to "Table" if applicable
        //    }
        //}
        private void LoadReportData(string deliveryNoteNo)
        {
            DataTable dt = GetReportData(deliveryNoteNo);

            if (dt.Rows.Count == 0)
            {
                this.DataSource = null;
                return;
            }

            this.DataSource = dt;
            this.DataMember = "";
          
            // Get currencyId from the first row
            if (dt.Columns.Contains("currencyId") && dt.Rows[0]["currencyId"] != DBNull.Value)
            {
                int currencyId = Convert.ToInt32(dt.Rows[0]["currencyId"]);
                string svgXml = GetCurrencySvgXml(currencyId);

                if (!string.IsNullOrEmpty(svgXml))
                {
                    foreach (string pictureBoxName in new[] {  "xrPictureBox2", "xrPictureBox3" })
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

        private void ApplyConditionalVisibility(bool showSeal, bool showSignature, bool printLetterhead, bool printFooterAtBottom)
        {

            // 🔹 Hide GroupFooter2 if PrintFooterAtBottom = true
            if (FindControl("GroupFooter2", true) is GroupFooterBand footerBand)
                footerBand.Visible = printFooterAtBottom;


            // 🔹 Seal logic (xrPictureBox1)
            if (FindControl("xrPictureBox1", true) is XRPictureBox sealPicture)
                sealPicture.Visible = showSeal;

            // 🔹 Signature logic (xrPictureBox5, xrPictureBox6, xrPictureBox7)
            foreach (string signatureBox in new[] { "xrPictureBox0", "xrPictureBox0", "xrPictureBox0" })
            {
                if (FindControl(signatureBox, true) is XRPictureBox sigBox)
                    sigBox.Visible = showSignature;
            }

            // 🔹 Letterhead logic (xrLabel75, xrLabel76, xrPictureBox11, xrLine3)
            if (FindControl("xrLabel76", true) is XRLabel lbl75)
                lbl75.Visible = printLetterhead;

            if (FindControl("xrLabel75", true) is XRLabel lbl76)
                lbl76.Visible = printLetterhead;

            if (FindControl("xrLabel68", true) is XRLabel lbl68)
                lbl68.Visible = printLetterhead;
            if (FindControl("xrLabel69", true) is XRLabel lbl69)
                lbl69.Visible = printLetterhead;
            if (FindControl("xrLabel70", true) is XRLabel lbl70)
                lbl70.Visible = printLetterhead;
            if (FindControl("xrLabel87", true) is XRLabel lbl87)
                lbl87.Visible = printLetterhead;
            if (FindControl("xrLabel88", true) is XRLabel lbl88)
                lbl88.Visible = printLetterhead;

            if (FindControl("xrPictureBox11", true) is XRPictureBox logoBox)
                logoBox.Visible = printLetterhead;

            if (FindControl("xrLine3", true) is XRLine line3)
                line3.Visible = printLetterhead;

            if (FindControl("xrLine7", true) is XRLine line7)
                line7.Visible = printLetterhead;
            if (FindControl("xrLine8", true) is XRLine line8)
                line8.Visible = printLetterhead;
        }

        private DataTable GetReportData(string deliveryNoteNo)
        {
            DataTable dt = new DataTable();

            try
            {
                if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    string connectionString = tenant.ConnectionString;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = "SELECT * FROM qry603_05DeliveryNoteReport WHERE DeliveryNoteNo = @DeliveryNoteNo";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@DeliveryNoteNo", deliveryNoteNo);

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
    }
}
