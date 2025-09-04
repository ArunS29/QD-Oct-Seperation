using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace QD.ERP.Web.Areas.IMS.Inventory_Reports
{
    public partial class PreviewQuotationwithoutPrice : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public PreviewQuotationwithoutPrice()
        {
            InitializeComponent();
        }

        public PreviewQuotationwithoutPrice(
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

            InitializeComponent();
            SetReportParameters(showSeal, showSignature, printLetterhead, pageBreakAfter, pageBreakBefore, printItemCodeDesc, printItemPartNoDesc, printItemPartArabicDesc, quotationNo, tenantName, companyName, logoImage, sealImage,
                                companyAddress, companyNameAr, companyAddressAr,username);
            LoadReportData(quotationNo);

            if (pageBreakBefore || pageBreakAfter)
                this.ReportFooter.PageBreak = DevExpress.XtraReports.UI.PageBreak.BeforeBand;
            else
                this.ReportFooter.PageBreak = DevExpress.XtraReports.UI.PageBreak.None;


            ApplyConditionalVisibility(showSeal, showSignature, printLetterhead);


            // Hide labels if clientAcknowledgement is true
            int[] labelNumbers = { 35, 36, 37, 41, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 70 };
            foreach (int num in labelNumbers)
            {
                var label = FindControl($"xrLabel{num}", true) as XRLabel;
                if (label != null)
                    label.Visible = clientAcknowledgement;
            }
        }

     

        private void SetReportParameters(bool showSeal, bool showSignature, bool printLetterhead, bool pageBreakAfter, bool pageBreakBefore, bool printItemCodeDesc, bool printItemPartNoDesc, bool printItemPartArabicDesc, string quotationNo, string tenantName, string companyName,
            Image logoImage, Image sealImage, string companyAddress, 
            string companyNameAr, string companyAddressAr,string username)
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

            if (FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = companyNameAr;

            if (FindControl("xrLabelCompanyAddressAr", true) is XRLabel addressArLabel)
                addressArLabel.Text = companyAddressAr;
            if (FindControl("UserName", true) is XRLabel usernameLabel)
                usernameLabel.Text = username;

            if (FindControl("xrPictureBox11", true) is XRPictureBox logoPictureBox)
                logoPictureBox.Image = logoImage;

            if (FindControl("xrPictureBox4", true) is XRPictureBox sealPictureBox)
                sealPictureBox.Image = sealImage;
        }

        private void LoadReportData(string quotationNo)
        {
            DataTable dt = GetReportData(quotationNo);

            if (dt.Rows.Count == 0)
            {
                this.DataSource = null;
            }
            else
            {
                this.DataSource = dt;
                this.DataMember = ""; // Optional: specify if using DataSet
            }
        }
        private void ApplyConditionalVisibility(bool showSeal, bool showSignature, bool printLetterhead)
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
            if (FindControl("xrLabel75", true) is XRLabel lbl75)
                lbl75.Visible = printLetterhead;

            if (FindControl("xrLabel76", true) is XRLabel lbl76)
                lbl76.Visible = printLetterhead;

            if (FindControl("xrPictureBox11", true) is XRPictureBox logoBox)
                logoBox.Visible = printLetterhead;

            if (FindControl("xrLine3", true) is XRLine line3)
                line3.Visible = printLetterhead;
        }
        private DataTable GetReportData(string quotationNo)
        {
            DataTable dt = new DataTable();

            try
            {
                if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
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
                    throw new Exception("Unable to get tenant context. Please check session and cache.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading quotation data: {ex.Message}");
            }

            return dt;
        }
    }
}
