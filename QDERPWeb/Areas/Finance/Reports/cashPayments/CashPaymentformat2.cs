using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using DevExpress.XtraReports.UI;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace QD.ERP.Web.Areas.Finance.Reports.cashPayments
{
    public partial class cashPaymentformat2 : XtraReport
    {
        public cashPaymentformat2(string voucherNo, string tenantName, string company_Name, string company_address,Image logoImage, string Company_Name_Ar, string company_address_arb)
        {
            InitializeComponent();
            SetReportParameters(voucherNo, tenantName, company_Name, company_address, logoImage, Company_Name_Ar, company_address_arb);
            LoadReportData(voucherNo);
        }

        private void SetReportParameters(string voucherNo, string tenantName, string company_Name, string company_address, Image logoImage, string Company_Name_Ar, string company_address_arb)
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
            AddOrUpdateParameter("CompanyName", company_Name ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddress", company_address ?? "", typeof(string));
            AddOrUpdateParameter("CompanyNameAr", Company_Name_Ar ?? "", typeof(string));
            AddOrUpdateParameter("CompanyAddressArb", company_address_arb ?? "", typeof(string));

            if (this.FindControl("xrLabelTenantName", true) is XRLabel tenantLabel)
            {
                tenantLabel.Text = tenantName;
            }
            if (this.FindControl("xrLabelCompanyName", true) is XRLabel companyNameLabel)
            {
                companyNameLabel.Text = company_Name;
            }
            if (this.FindControl("xrLabelCompanyAddress", true) is XRLabel addressLabel)
            {
                addressLabel.Text = company_address;
            }
            if (this.FindControl("xrLabelCompanyNameAr", true) is XRLabel companyNameArLabel)
            {
                companyNameArLabel.Text = Company_Name_Ar;
            }
            if (this.FindControl("xrLabelCompanyAddressArb", true) is XRLabel addressArbLabel)
            {
                addressArbLabel.Text = company_address_arb;
            }
            if (this.FindControl("xrPictureBox1", true) is XRPictureBox logoPictureBox)
            {
                logoPictureBox.Image = logoImage;
            }
        }

        private void LoadReportData(string voucherNo)
        {
            DataTable dt = GetReportData(voucherNo);

            if (dt.Rows.Count == 0)
            {
                this.DataSource = null;
                this.CreateNoDataLabel();
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
                BoundsF = new System.Drawing.RectangleF(0, 0, PageWidth - Margins.Left - Margins.Right, 50),
                TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleCenter
            };
            this.Bands[BandKind.Detail].Controls.Add(noDataLabel);
        }
    }
}