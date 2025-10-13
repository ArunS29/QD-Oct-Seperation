using DevExpress.XtraPrinting.Drawing;
using DevExpress.XtraReports.UI;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;

namespace QD.ERP.Web.Areas.ERM.Reports.Enquiry
{
    public partial class PreviewRequests : DevExpress.XtraReports.UI.XtraReport
    {
        private readonly TenantDbContextHelper _tenantDbContextHelper;

        public PreviewRequests()
        {
            InitializeComponent();
        }

        public PreviewRequests(
             bool showSeal,
            bool showSignature,
            bool printLetterhead,
            string RequestNo,
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
            SetReportParameters(showSeal, showSignature, printLetterhead, RequestNo, tenantName, companyName, logoImage, sealImage, companyAddress, companyNameAr, companyAddressAr, username);
            LoadReportData(RequestNo);
            ApplyConditionalVisibility(showSeal, showSignature, printLetterhead);

        }

        private void SetReportParameters(bool showSeal, bool showSignature, bool printLetterhead,
            string RequestNo, string tenantName, string companyName, Image logoImage, Image sealImage,
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

            AddOrUpdateParameter("RequestNo", RequestNo, typeof(string));
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

        private void LoadReportData(string RequestNo)
        {
            DataTable dt = GetReportData(RequestNo);

            if (dt.Rows.Count == 0)
            {
                this.DataSource = null;
            }
            else
            {
                this.DataSource = dt;
                this.DataMember = ""; // Optional: set if you use named dataset
            }
        }

        private void ApplyConditionalVisibility(bool showSeal, bool showSignature, bool printLetterhead)
        {
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
            if (FindControl("xrLabel53", true) is XRLabel lbl53)
                lbl53.Visible = printLetterhead;

            if (FindControl("xrLabel47", true) is XRLabel lbl47)
                lbl47.Visible = printLetterhead;
            if (FindControl("xrLabel88", true) is XRLabel lbl88)
                lbl88.Visible = printLetterhead;
            if (FindControl("xrLabel68", true) is XRLabel lbl68)
                lbl68.Visible = printLetterhead;
            if (FindControl("xrLabel69", true) is XRLabel lbl69)
                lbl69.Visible = printLetterhead;
            if (FindControl("xrLabel70", true) is XRLabel lbl70)
                lbl70.Visible = printLetterhead;
            if (FindControl("xrLabel87", true) is XRLabel lbl87)
                lbl87.Visible = printLetterhead;

            if (FindControl("xrPictureBox11", true) is XRPictureBox logoBox)
                logoBox.Visible = printLetterhead;

            if (FindControl("xrLine3", true) is XRLine line3)
                line3.Visible = printLetterhead;
        }
        
        private DataTable GetReportData(string RequestNo)
        {
            DataTable dt = new DataTable();

            try
            {
                if (_tenantDbContextHelper != null && _tenantDbContextHelper.TryGetTenantAndDbContext(out var tenant, out var _))
                {
                    string connectionString = tenant.ConnectionString;

                    using (SqlConnection conn = new SqlConnection(connectionString))
                    {
                      
                        string schemaName = string.IsNullOrWhiteSpace(tenant.schemaname) ? "dbo" : tenant.schemaname;
                        string query = $"SELECT * FROM {schemaName}.qry406_05PropertyRequestReport WHERE EqiupmentRequestNo = @RequestNo";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@RequestNo", RequestNo);

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
                Console.WriteLine($"Error fetching RFQ report data: {ex.Message}");
             
                throw new Exception("Error loading report data: " + ex.Message, ex);
            }

            return dt;
        }
    }
}
