using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace QD.ERP.Web.Areas.IMS.Reports.InventoryReports
{
    public partial class ReportforMaterialIssueNote : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;
        private string _deliveryNoteNo;

        public ReportforMaterialIssueNote()
        {
            InitializeComponent();
        }

        public ReportforMaterialIssueNote(
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
            _deliveryNoteNo = deliveryNoteNo; // <-- ADD THIS LINE

            InitializeComponent();
            SetReportParameters(PrintFooterAtBottom, ShowItemLineNo, printItemPartArabicDesc, showSeal, showSignature, printLetterhead, printItemCodeDesc, printItemPartNoDesc, deliveryNoteNo, tenantName, companyName, logoImage, sealImage, companyAddress, companyNameAr, companyAddressAr, username);
            LoadReportData(deliveryNoteNo);
            ApplyConditionalVisibility(showSeal, showSignature, printLetterhead, PrintFooterAtBottom);

            this.BeforePrint += MaterialRequestInventory_BeforePrint;
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

        private void LoadReportData(string deliveryNoteNo)
        {
            DataTable dt = GetReportData(deliveryNoteNo);

            if (dt.Rows.Count == 0)
            {
                this.DataSource = null;
            }
            else
            {
                this.DataSource = dt;
                this.DataMember = ""; // Set if you use a named data source
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
            foreach (string signatureBox in new[] { "xrPictureBox2", "xrPictureBox0", "xrPictureBox0" })
            {
                if (FindControl(signatureBox, true) is XRPictureBox sigBox)
                    sigBox.Visible = showSignature;
            }

            // 🔹 Letterhead logic (xrLabel75, xrLabel76, xrPictureBox11, xrLine3)
            if (FindControl("xrLabel85", true) is XRLabel lbl75)
                lbl75.Visible = printLetterhead;

            if (FindControl("xrLabel86", true) is XRLabel lbl76)
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
                        string query = "SELECT * FROM qry603_05DeliveryNoteReport  WHERE DeliveryNoteNo = @DeliveryNoteNo";

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

        private int GetTypeOfMPR(string deliveryNoteNo)
        {
            int deliveryType = 0;

            try
            {
                if (_tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    string connectionString = tenant.ConnectionString;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                        string query = "SELECT ISNULL(DeliveryType,0) FROM tbl603_01DeliveryNoteMaster WHERE DeliveryNoteNo = @DeliveryNoteNo";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@DeliveryNoteNo", deliveryNoteNo);

                            conn.Open();
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                                deliveryType = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching TypeOfMPR: {ex.Message}");
            }

            return deliveryType;
        }
        private void SetLabelsVisibility(int deliveryType)
        {
            // ================= CASE 1 (4–17) =================
            XRLabel lbl4 = FindControl("xrLabel4", true) as XRLabel;
            XRLabel lbl5 = FindControl("xrLabel5", true) as XRLabel;
            XRLabel lbl6 = FindControl("xrLabel6", true) as XRLabel;
            XRLabel lbl7 = FindControl("xrLabel7", true) as XRLabel;
            XRLabel lbl8 = FindControl("xrLabel8", true) as XRLabel;
            XRLabel lbl9 = FindControl("xrLabel9", true) as XRLabel;
            XRLabel lbl10 = FindControl("xrLabel10", true) as XRLabel;
            XRLabel lbl11 = FindControl("xrLabel11", true) as XRLabel;
            XRLabel lbl12 = FindControl("xrLabel12", true) as XRLabel;
            XRLabel lbl13 = FindControl("xrLabel13", true) as XRLabel;
            XRLabel lbl14 = FindControl("xrLabel14", true) as XRLabel;
            XRLabel lbl15 = FindControl("xrLabel15", true) as XRLabel;
            XRLabel lbl16 = FindControl("xrLabel16", true) as XRLabel;
            XRLabel lbl17 = FindControl("xrLabel17", true) as XRLabel;

            // ================= CASE 2 (57–62, 66–70) =================
            XRLabel lbl57 = FindControl("xrLabel57", true) as XRLabel;
            XRLabel lbl58 = FindControl("xrLabel58", true) as XRLabel;
            XRLabel lbl59 = FindControl("xrLabel59", true) as XRLabel;
            XRLabel lbl60 = FindControl("xrLabel60", true) as XRLabel;
            XRLabel lbl61 = FindControl("xrLabel61", true) as XRLabel;
            XRLabel lbl62 = FindControl("xrLabel62", true) as XRLabel;
            XRLabel lbl66 = FindControl("xrLabel66", true) as XRLabel;
            XRLabel lbl67 = FindControl("xrLabel67", true) as XRLabel;
            XRLabel lbl68 = FindControl("xrLabel68", true) as XRLabel;
            XRLabel lbl69 = FindControl("xrLabel69", true) as XRLabel;
            XRLabel lbl70 = FindControl("xrLabel70", true) as XRLabel;

            // ================= CASE 3 (63–65, 71–78) =================
            XRLabel lbl63 = FindControl("xrLabel63", true) as XRLabel;
            XRLabel lbl64 = FindControl("xrLabel64", true) as XRLabel;
            XRLabel lbl65 = FindControl("xrLabel65", true) as XRLabel;
            XRLabel lbl71 = FindControl("xrLabel71", true) as XRLabel;
            XRLabel lbl72 = FindControl("xrLabel72", true) as XRLabel;
            XRLabel lbl73 = FindControl("xrLabel73", true) as XRLabel;
            XRLabel lbl74 = FindControl("xrLabel74", true) as XRLabel;
            XRLabel lbl75 = FindControl("xrLabel75", true) as XRLabel;
            XRLabel lbl76 = FindControl("xrLabel76", true) as XRLabel;
            XRLabel lbl77 = FindControl("xrLabel77", true) as XRLabel;
            XRLabel lbl78 = FindControl("xrLabel78", true) as XRLabel;

            // ================= CASE 4 (79–84) =================
            XRLabel lbl79 = FindControl("xrLabel79", true) as XRLabel;
            XRLabel lbl80 = FindControl("xrLabel80", true) as XRLabel;
            XRLabel lbl81 = FindControl("xrLabel81", true) as XRLabel;
            XRLabel lbl82 = FindControl("xrLabel82", true) as XRLabel;
            XRLabel lbl83 = FindControl("xrLabel83", true) as XRLabel;
            XRLabel lbl84 = FindControl("xrLabel84", true) as XRLabel;

            // 🔹 Hide all labels first
            foreach (var lbl in new XRLabel[] {
        lbl4,lbl5,lbl6,lbl7,lbl8,lbl9,lbl10,lbl11,lbl12,lbl13,lbl14,lbl15,lbl16,lbl17,
        lbl57,lbl58,lbl59,lbl60,lbl61,lbl62,lbl66,lbl67,lbl68,lbl69,lbl70,
        lbl63,lbl64,lbl65,lbl71,lbl72,lbl73,lbl74,lbl75,lbl76,lbl77,lbl78,
        lbl79,lbl80,lbl81,lbl82,lbl83,lbl84
    })
            {
                if (lbl != null) lbl.Visible = false;
            }

            // 🔹 Show based on deliveryType
            switch (deliveryType)
            {
                case 1: // show labels 4–17
                    ShowLabels(lbl4, lbl5, lbl6, lbl7, lbl8, lbl9, lbl10, lbl11, lbl12, lbl13, lbl14, lbl15, lbl16, lbl17);
                    break;

                case 2: // show labels 57–62,66–70
                    ShowLabels(lbl57, lbl58, lbl59, lbl60, lbl61, lbl62, lbl66, lbl67, lbl68, lbl69, lbl70);
                    break;

                case 3: // show labels 63–65,71–78
                    ShowLabels(lbl63, lbl64, lbl65, lbl71, lbl72, lbl73, lbl74, lbl75, lbl76, lbl77, lbl78);
                    break;

                case 4: // show labels 79–84
                    ShowLabels(lbl79, lbl80, lbl81, lbl82, lbl83, lbl84);
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine($"deliveryType '{deliveryType}' did not match any label group.");
                    break;
            }
        }

      
        private void ShowLabels(params XRLabel[] labels)
        {
            foreach (var lbl in labels)
            {
                if (lbl != null)
                {
                    lbl.Visible = true;
                    System.Diagnostics.Debug.WriteLine($"{lbl.Name} set to Visible");
                }
            }
        }

        // Ensure the handler is 'protected' or 'public' and matches the expected signature for BeforePrintEventHandler
        private void MaterialRequestInventory_BeforePrint(object sender, EventArgs e)
        {
            int deliveryType = GetTypeOfMPR(_deliveryNoteNo);
            SetLabelsVisibility(deliveryType);

            
        }
    }
}

