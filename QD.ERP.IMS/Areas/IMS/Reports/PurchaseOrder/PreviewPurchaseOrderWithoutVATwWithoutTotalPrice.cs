using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Text;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Microsoft.Extensions.Configuration;

using QD.ERP.Shared.Service;
using Svg;

namespace QD.ERP.IMS.Areas.IMS.Reports.InventroryReports.PurchaseOrder
{
    public partial class PreviewPurchaseOrderWithoutVATwWithoutTotalPrice : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private readonly Tenant _resolvedTenant;
        private HashSet<string> _quoteNosWithTerms = new HashSet<string>();


        public PreviewPurchaseOrderWithoutVATwWithoutTotalPrice()
        {
            InitializeComponent();
        }

        public PreviewPurchaseOrderWithoutVATwWithoutTotalPrice(
            string companyPhone,
            string companyEmail,
            string companyWebsite,
            Image logoImage,
            bool ShowFullSupplierAcceptance,
            bool ShowSimpleSuppilerAcceptance,
            bool ShowSignatoryPositionOnly,
            bool ShowPaymentTermsShippingDetails,
            bool ShowitemPartNumberinsteadStockCode,
            bool ShowHSCodeinsteadStockCode,
            bool showSeal,
            bool showSignature,
            bool printLetterhead,
             bool pageBreakBefore,
             bool pageBreakAfter,
            string purchaseNo,
            string tenantName,
            string companyName,
          
            Image sealImage,
            string companyAddress,
            string companyNameAr,
            string companyAddressAr,
            TenantDbContextHelper tenantDbContextHelper)
        {
            _tenantDbContextHelper = tenantDbContextHelper;

            if (!_tenantDbContextHelper.TryGetTenantAndDbContext(out _resolvedTenant, out _))
                throw new Exception("Unable to resolve tenant context during report creation.");

            InitializeComponent();
            SetReportParameters(companyPhone, companyEmail, companyWebsite, logoImage, ShowFullSupplierAcceptance, ShowSimpleSuppilerAcceptance, ShowSignatoryPositionOnly, ShowPaymentTermsShippingDetails, ShowitemPartNumberinsteadStockCode, ShowHSCodeinsteadStockCode, showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, purchaseNo, tenantName, companyName, sealImage, companyAddress, companyNameAr, companyAddressAr);
            LoadReportData(purchaseNo);

            LoadSubreport(purchaseNo);
            xrSubreport1.BeforePrint += xrSubreport1_BeforePrint;


            ApplyConditionalVisibility(showSeal, showSignature, printLetterhead, ShowitemPartNumberinsteadStockCode, ShowHSCodeinsteadStockCode, ShowFullSupplierAcceptance, ShowSimpleSuppilerAcceptance, ShowSignatoryPositionOnly);

            // Show/hide GroupFooter3 based on ShowPaymentTermsShippingDetails
            if (Bands["GroupFooter3"] != null)
                Bands["GroupFooter3"].Visible = ShowPaymentTermsShippingDetails;

            if (pageBreakBefore || pageBreakAfter)
                this.ReportFooter.PageBreak = DevExpress.XtraReports.UI.PageBreak.BeforeBand;
            else
                this.ReportFooter.PageBreak = DevExpress.XtraReports.UI.PageBreak.None;
        }

        private void SetReportParameters(string companyPhone, string companyEmail, string companyWebsite, Image logoImage, bool ShowFullSupplierAcceptance, bool ShowSimpleSuppilerAcceptance, bool ShowSignatoryPositionOnly, bool ShowPaymentTermsShippingDetails, bool ShowitemPartNumberinsteadStockCode, bool ShowHSCodeinsteadStockCode, bool showSeal, bool showSignature, bool printLetterhead, bool pageBreakAfter, bool pageBreakBefore, string purchaseNo, string tenantName, string companyName, Image sealImage,
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
            AddOrUpdateParameter("CompanyPhone", companyPhone ?? "", typeof(string));
            AddOrUpdateParameter("CompanyEmailAddress", companyEmail ?? "", typeof(string));

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
            if (FindControl("xrPictureBox13", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;
            if (FindControl("xrLabelCompanyPhone", true) is XRLabel companyphoneLabel)
                companyphoneLabel.Text = companyPhone;

            if (FindControl("xrLabelCompanyEmailAddress", true) is XRLabel emailLabel)
                emailLabel.Text = companyEmail;

            if (FindControl("xrLabelCompanyWebsite", true) is XRLabel websiteLabel)
                websiteLabel.Text = companyWebsite;


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
        //    }
        //}
        private void ApplyConditionalVisibility(
bool showSeal,
bool showSignature,
bool printLetterhead,
bool ShowitemPartNumberinsteadStockCode,
bool ShowHSCodeinsteadStockCode,
bool ShowFullSupplierAcceptance,
bool ShowSimpleSuppilerAcceptance,
bool ShowSignatoryPositionOnly
)
        {
            // 🔹 Seal logic (xrPictureBox1)
            if (FindControl("xrPictureBox4", true) is XRPictureBox sealPicture)
                sealPicture.Visible = showSeal;

            // 🔹 Signature logic (xrPictureBox5, xrPictureBox6, xrPictureBox7)
            foreach (string signatureBox in new[] { "xrPictureBox1", "xrPictureBox2", "xrPictureBox3" })
            {
                if (FindControl(signatureBox, true) is XRPictureBox sigBox)
                    sigBox.Visible = showSignature;
            }

            // 🔹 Letterhead logic (xrLabel75, xrLabel76, xrPictureBox11, xrLine3)
            if (FindControl("xrLabel72", true) is XRLabel lbl72)
                lbl72.Visible = printLetterhead;

            if (FindControl("xrLabel77", true) is XRLabel lbl77)
                lbl77.Visible = printLetterhead;
            if (FindControl("xrLabel87", true) is XRLabel lbl87)
                lbl87.Visible = printLetterhead;
            if (FindControl("xrLabel88", true) is XRLabel lbl88)
                lbl88.Visible = printLetterhead;
            if (FindControl("xrLabel86", true) is XRLabel lbl86)
                lbl86.Visible = printLetterhead;
            if (FindControl("xrLabel85", true) is XRLabel lbl85)
                lbl85.Visible = printLetterhead;
            if (FindControl("xrLabel83", true) is XRLabel lbl83)
                lbl83.Visible = printLetterhead;

            if (FindControl("xrPictureBox13", true) is XRPictureBox logoBox)
                logoBox.Visible = printLetterhead;

            if (FindControl("xrLine4", true) is XRLine line4)
                line4.Visible = printLetterhead;

            // --- Custom logic for your requirement ---

            // Hide all first
            foreach (string lbl in new[] { "xrLabel19", "xrLabel26", "xrLabel45", "xrLabel79", "xrLabel78", "xrLabel80" })
            {
                if (FindControl(lbl, true) is XRLabel label)
                    label.Visible = false;
            }

            if (ShowitemPartNumberinsteadStockCode && ShowHSCodeinsteadStockCode)
            {
                // Both true: show 50 and 51
                if (FindControl("xrLabel26", true) is XRLabel l26) l26.Visible = true;
                if (FindControl("xrLabel79", true) is XRLabel l79) l79.Visible = true;
            }
            else if (ShowitemPartNumberinsteadStockCode)
            {
                // Only item part number: show 52 and 76
                if (FindControl("xrLabel19", true) is XRLabel l19) l19.Visible = true;
                if (FindControl("xrLabel80", true) is XRLabel l80) l80.Visible = true;
            }
            else if (ShowHSCodeinsteadStockCode)
            {
                // Only HS code: show 50 and 50
                if (FindControl("xrLabel26", true) is XRLabel l26) l26.Visible = true;
                if (FindControl("xrLabel79", true) is XRLabel l79) l79.Visible = true;
            }
            else
            {
                // Both false: show 24 and 34
                if (FindControl("xrLabel19", true) is XRLabel l19) l19.Visible = true;
                if (FindControl("xrLabel78", true) is XRLabel l78) l78.Visible = true;
            }

            // Show/hide xrPanel1 based on ShowFullSupplierAcceptance
            if (FindControl("xrPanel1", true) is XRPanel panel1)
                panel1.Visible = ShowFullSupplierAcceptance;

            // Show/hide panelAcceptance02 based on ShowSimpleSuppilerAcceptance
            if (FindControl("panelAcceptance02", true) is XRPanel panel2)
                panel2.Visible = ShowSimpleSuppilerAcceptance;

            // Hide or show xrPanel2, xrPanel3, xrPanel4 based on ShowSignatoryPositionOnly
            foreach (string panelName in new[] { "xrPanel2", "xrPanel3", "xrPanel4" })
            {
                if (FindControl(panelName, true) is XRPanel panel)
                    panel.Visible = !ShowSignatoryPositionOnly;
            }
        }

        private void xrSubreport1_BeforePrint(object sender, EventArgs e)
        {
            // Get SupplierQuoteNo from the current row/group
            string supplierQuoteNo = GetCurrentColumnValue("SupplierQuoteNo")?.ToString();

            var tenant = _resolvedTenant;
            var subReport = new quotationstoClients.IMSContext();
            if (!string.IsNullOrWhiteSpace(supplierQuoteNo) && tenant != null)
            {
                // Pass SupplierQuoteNo as QuoteNo to the subreport
                subReport.LoadTerms(supplierQuoteNo.Trim(), tenant.ConnectionString);
                xrSubreport1.ReportSource = subReport;

                // Show subreport only if data exists
                var dt = subReport.DataSource as DataTable;
                xrSubreport1.Visible = dt != null && dt.Rows.Count > 0;
            }
            else
            {
                xrSubreport1.Visible = false;
            }
        }
        private void LoadSubreport(string purchaseOrderNo)
        {
            if (string.IsNullOrWhiteSpace(purchaseOrderNo))
                return;

            var tenant = _resolvedTenant ?? throw new Exception("Tenant not resolved.");
            DataTable dt = new DataTable();

            using (var conn = new SqlConnection(tenant.ConnectionString))
            {
                // Get all distinct SupplierQuoteNo for this purchase order
                string query = @"
                    SELECT DISTINCT LTRIM(RTRIM(SupplierQuoteNo)) AS SupplierQuoteNo
                    FROM qry604_05PurchaseOrderReport
                    WHERE PONo = @PurchaseOrderNo AND SupplierQuoteNo IS NOT NULL";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PurchaseOrderNo", purchaseOrderNo);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    adapter.Fill(dt);
                }
            }

            _quoteNosWithTerms.Clear();
            foreach (DataRow row in dt.Rows)
            {
                string supplierQuoteNo = row["SupplierQuoteNo"]?.ToString();
                if (!string.IsNullOrWhiteSpace(supplierQuoteNo))
                {
                    _quoteNosWithTerms.Add(supplierQuoteNo.Trim());
                }
            }
        }
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
            

            // Get currencyId from the first row
            if (dt.Columns.Contains("currencyId") && dt.Rows[0]["currencyId"] != DBNull.Value)
            {
                int currencyId = Convert.ToInt32(dt.Rows[0]["currencyId"]);
                string svgXml = GetCurrencySvgXml(currencyId);

                if (!string.IsNullOrEmpty(svgXml))
                {
                    foreach (string pictureBoxName in new[] {  "xrPictureBox5" })
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
        private void PreviewPurchaseOrderWithoutVATwWithoutTotalPrice_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Custom logic before print if needed
        }
    }
}

