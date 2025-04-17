
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using Microsoft.Extensions.Configuration;
using DevExpress.XtraReports.UI;

namespace QD.ERP.Web.Areas.Finance.Reports.ExpensesClaims
{
    public partial class PreviewClaimRequestForm_wtVAT_ : XtraReport
    {
        public PreviewClaimRequestForm_wtVAT_(
            string voucherNo,
            string tenantName,
            string companyName,
            string companyAddress,
            Image logoImage,
            string companyNameAr,
            string companyAddressAr)
        {
            InitializeComponent();
            SetReportParameters(voucherNo, tenantName, companyName, companyAddress, logoImage, companyNameAr, companyAddressAr);
            LoadReportData(voucherNo);
        }

        private void SetReportParameters(string voucherNo, string tenantName, string companyName, string companyAddress, Image logoImage, string companyNameAr, string companyAddressAr)
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
            AddOrUpdateParameter("CompanyName", companyName ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", companyAddress ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", companyNameAr ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressAr", companyAddressAr ?? "", typeof(string));

            if (this.FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
                tenantLabel.Text = tenantName;

            if (this.FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
                companyNameLabel.Text = companyName;

            if (this.FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
                addressLabel.Text = companyAddress;

            if (this.FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
                companyNameArLabel.Text = companyNameAr;

            if (this.FindControl("xrLabelCompanyAddressAr", true) is XRLabel addressArLabel)
                addressArLabel.Text = companyAddressAr;

            if (this.FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
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
            }
        }

        private DataTable GetReportData(string voucherNo)
        {
            DataTable dt = new DataTable();

            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                string connectionString = configuration.GetConnectionString("DbConnection");

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT * FROM tbl20103ExpenseClaimChild WHERE ClaimRefNo=@ClaimRefNo";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.CommandType = CommandType.Text; // 👈 Fixed here
                        cmd.Parameters.AddWithValue("@ClaimRefNo", voucherNo);

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

        private void xrTableCell9_BeforePrint(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}

